using System;
using System.Drawing;
using System.Windows.Forms;
using BugShooting.Output.SpiraTest.SoapService;
using System.Threading.Tasks;
using System.ServiceModel;
using BS.Plugin.V3.Output;
using BS.Plugin.V3.Common;
using BS.Plugin.V3.Utilities;

namespace BugShooting.Output.SpiraTest
{
  public class OutputPlugin: OutputPlugin<Output>
  {

    protected override string Name
    {
      get { return "SpiraTest"; }
    }

    protected override Image Image64
    {
      get  { return Properties.Resources.logo_64; }
    }

    protected override Image Image16
    {
      get { return Properties.Resources.logo_16 ; }
    }

    protected override bool Editable
    {
      get { return true; }
    }

    protected override string Description
    {
      get { return "Attach screenshots to SpiraTest items."; }
    }
    
    protected override Output CreateOutput(IWin32Window Owner)
    {
      
      Output output = new Output(Name, 
                                 String.Empty, 
                                 String.Empty, 
                                 String.Empty, 
                                 "Screenshot",
                                 String.Empty, 
                                 true,
                                 1,
                                 ItemType.Incident,
                                 1);

      return EditOutput(Owner, output);

    }

    protected override Output EditOutput(IWin32Window Owner, Output Output)
    {

      Edit edit = new Edit(Output);

      var ownerHelper = new System.Windows.Interop.WindowInteropHelper(edit);
      ownerHelper.Owner = Owner.Handle;
      
      if (edit.ShowDialog() == true) {

        return new Output(edit.OutputName,
                          edit.Url,
                          edit.UserName,
                          edit.Password,
                          edit.FileName,
                          edit.FileFormat,
                          edit.OpenItemInBrowser,
                          Output.LastProjectID,
                          Output.LastItemType,
                          Output.LastItemID);
      }
      else
      {
        return null; 
      }

    }

    protected override OutputValues SerializeOutput(Output Output)
    {

      OutputValues outputValues = new OutputValues();

      outputValues.Add("Name", Output.Name);
      outputValues.Add("Url", Output.Url);
      outputValues.Add("UserName", Output.UserName);
      outputValues.Add("Password",Output.Password, true);
      outputValues.Add("OpenItemInBrowser", Convert.ToString(Output.OpenItemInBrowser));
      outputValues.Add("FileName", Output.FileName);
      outputValues.Add("FileFormat", Output.FileFormat);
      outputValues.Add("LastProjectID", Output.LastProjectID.ToString());
      outputValues.Add("LastItemType", Output.LastItemType.ToString());
      outputValues.Add("LastItemID", Output.LastItemID.ToString());

      return outputValues;
      
    }

    protected override Output DeserializeOutput(OutputValues OutputValues)
    {

      return new Output(OutputValues["Name", this.Name],
                        OutputValues["Url", ""],
                        OutputValues["UserName", ""],
                        OutputValues["Password", ""],
                        OutputValues["FileName", "Screenshot"],
                        OutputValues["FileFormat", ""],
                        Convert.ToBoolean(OutputValues["OpenItemInBrowser", Convert.ToString(true)]),
                        Convert.ToInt32(OutputValues["LastProjectID", "1"]),
                        (ItemType)Enum.Parse(typeof(ItemType), OutputValues["LastItemType", "1"]),
                        Convert.ToInt32(OutputValues["LastItemID", "1"]));

    }

    protected override async Task<SendResult> Send(IWin32Window Owner, Output Output, ImageData ImageData)
    {

      try
      {

        HttpBindingBase binding;
        if (Output.Url.StartsWith("https", StringComparison.InvariantCultureIgnoreCase))
        {
          binding = new BasicHttpsBinding();
        }
        else
        {
          binding = new BasicHttpBinding();
        }
        binding.MaxBufferSize = int.MaxValue;
        binding.MaxReceivedMessageSize = int.MaxValue;
        binding.AllowCookies = true;

        SoapServiceClient spiraTestClient = new SoapServiceClient(binding, new EndpointAddress(Output.Url + "/Services/v5_0/SoapService.svc"));


        string userName = Output.UserName;
        string password = Output.Password;
        bool showLogin = string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password);
        bool rememberCredentials = false;

        string fileName = AttributeHelper.ReplaceAttributes(Output.FileName, ImageData);

        while (true)
        {

          if (showLogin)
          {

            // Show credentials window
            Credentials credentials = new Credentials(Output.Url, userName, password, rememberCredentials);

            var credentialsOwnerHelper = new System.Windows.Interop.WindowInteropHelper(credentials);
            credentialsOwnerHelper.Owner = Owner.Handle;

            if (credentials.ShowDialog() != true)
            {
              return new SendResult(Result.Canceled);
            }

            userName = credentials.UserName;
            password = credentials.Password;
            rememberCredentials = credentials.Remember;

          }

          if (! await spiraTestClient.Connection_AuthenticateAsync(userName, password))
          {
            showLogin = true;
            continue;
          }


          // Get available projects
          RemoteProject[] projects = await spiraTestClient.Project_RetrieveAsync();
          

          // Show send window
          Send send = new Send(Output.Url, Output.LastProjectID, Output.LastItemType, Output.LastItemID, projects, fileName);

          var sendOwnerHelper = new System.Windows.Interop.WindowInteropHelper(send);
          sendOwnerHelper.Owner = Owner.Handle;

          if (!send.ShowDialog() == true)
          {
            return new SendResult(Result.Canceled);
          }
          

          // Upload file
          string fullFileName = String.Format("{0}.{1}", send.FileName, FileHelper.GetFileExtention(Output.FileFormat));
          byte[] fileBytes = FileHelper.GetFileBytes(Output.FileFormat, ImageData);

          RemoteDocument document = new RemoteDocument();
          document.FilenameOrUrl = fullFileName;
          document.Description = send.Comment;

          await spiraTestClient.Connection_ConnectToProjectAsync(send.ProjectID);

          document = await spiraTestClient.Document_AddFileAsync(document, fileBytes);

          await spiraTestClient.Document_AddToArtifactIdAsync((int)send.ItemType, send.ItemID, document.AttachmentId.Value);


          // Open item in browser
          if (Output.OpenItemInBrowser)
          {
            WebHelper.OpenUrl(String.Format("{0}/{1}/{2}/{3}.aspx", Output.Url, send.ProjectID, send.ItemType.ToString(), send.ItemID));
          }


          return new SendResult(Result.Success,
                                new Output(Output.Name,
                                           Output.Url,
                                           (rememberCredentials) ? userName : Output.UserName,
                                           (rememberCredentials) ? password : Output.Password,
                                           Output.FileName,
                                           Output.FileFormat,
                                           Output.OpenItemInBrowser,
                                           send.ProjectID,
                                           send.ItemType,
                                           send.ItemID));
                  
        }

      }
      catch (Exception ex)
      {
        return new SendResult(Result.Failed, ex.Message);
      }

    }

  }
}
