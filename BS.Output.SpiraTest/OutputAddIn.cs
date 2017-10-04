using System;
using System.Drawing;
using System.Windows.Forms;
using BS.Output.SpiraTest.SpiraTest.SoapService;
using System.Threading.Tasks;
using System.ServiceModel;

namespace BS.Output.SpiraTest
{
  public class OutputAddIn: V3.OutputAddIn<Output>
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

    protected override OutputValueCollection SerializeOutput(Output Output)
    {

      OutputValueCollection outputValues = new OutputValueCollection();

      outputValues.Add(new OutputValue("Name", Output.Name));
      outputValues.Add(new OutputValue("Url", Output.Url));
      outputValues.Add(new OutputValue("UserName", Output.UserName));
      outputValues.Add(new OutputValue("Password",Output.Password, true));
      outputValues.Add(new OutputValue("OpenItemInBrowser", Convert.ToString(Output.OpenItemInBrowser)));
      outputValues.Add(new OutputValue("FileName", Output.FileName));
      outputValues.Add(new OutputValue("FileFormat", Output.FileFormat));
      outputValues.Add(new OutputValue("LastProjectID", Output.LastProjectID.ToString()));
      outputValues.Add(new OutputValue("LastItemType", Output.LastItemType.ToString()));
      outputValues.Add(new OutputValue("LastItemID", Output.LastItemID.ToString()));

      return outputValues;
      
    }

    protected override Output DeserializeOutput(OutputValueCollection OutputValues)
    {

      return new Output(OutputValues["Name", this.Name].Value,
                        OutputValues["Url", ""].Value,
                        OutputValues["UserName", ""].Value,
                        OutputValues["Password", ""].Value,
                        OutputValues["FileName", "Screenshot"].Value,
                        OutputValues["FileFormat", ""].Value,
                        Convert.ToBoolean(OutputValues["OpenItemInBrowser", Convert.ToString(true)].Value),
                        Convert.ToInt32(OutputValues["LastProjectID", "1"].Value),
                        (ItemType)Enum.Parse(typeof(ItemType), OutputValues["LastItemType", "1"].Value),
                        Convert.ToInt32(OutputValues["LastItemID", "1"].Value));

    }

    protected override async Task<V3.SendResult> Send(IWin32Window Owner, Output Output, V3.ImageData ImageData)
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

        string fileName = V3.FileHelper.GetFileName(Output.FileName, Output.FileFormat, ImageData);

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
              return new V3.SendResult(V3.Result.Canceled);
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
            return new V3.SendResult(V3.Result.Canceled);
          }
          

          // Upload file
          string fullFileName = String.Format("{0}.{1}", send.FileName, V3.FileHelper.GetFileExtention(Output.FileFormat));
          byte[] fileBytes = V3.FileHelper.GetFileBytes(Output.FileFormat, ImageData);

          RemoteDocument document = new RemoteDocument();
          document.FilenameOrUrl = fullFileName;
          document.Description = send.Comment;

          await spiraTestClient.Connection_ConnectToProjectAsync(send.ProjectID);

          document = await spiraTestClient.Document_AddFileAsync(document, fileBytes);

          await spiraTestClient.Document_AddToArtifactIdAsync((int)send.ItemType, send.ItemID, document.AttachmentId.Value);


          // Open item in browser
          if (Output.OpenItemInBrowser)
          {
            V3.WebHelper.OpenUrl(String.Format("{0}/{1}/{2}/{3}.aspx", Output.Url, send.ProjectID, send.ItemType.ToString(), send.ItemID));
          }


          return new V3.SendResult(V3.Result.Success,
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
        return new V3.SendResult(V3.Result.Failed, ex.Message);
      }

    }

  }
}
