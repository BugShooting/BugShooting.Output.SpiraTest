namespace BS.Output.SpiraTest
{

  public enum ItemType
  {
    Requirement = 1,
    TestCase = 2,
    Incident = 3,
    Release = 4,
    TestRun = 5,
    Task = 6,
    TestStep = 7,
    TestSet = 8
  }

  public class Output: IOutput 
  {
    
    string name;
    string url;
    string userName;
    string password;
    string fileName;
    string fileFormat;
    bool openItemInBrowser;
    int lastProjectID;
    ItemType lastItemType;
    int lastItemID;

    public Output(string name, 
                  string url, 
                  string userName,
                  string password, 
                  string fileName, 
                  string fileFormat,
                  bool openItemInBrowser, 
                  int lastProjectID,
                  ItemType lastItemType,
                  int lastItemID)
    {
      this.name = name;
      this.url = url;
      this.userName = userName;
      this.password = password;
      this.fileName = fileName;
      this.fileFormat = fileFormat;
      this.openItemInBrowser = openItemInBrowser;
      this.lastProjectID = lastProjectID;
      this.lastItemType = lastItemType;
      this.lastItemID = lastItemID;
    }
    
    public string Name
    {
      get { return name; }
    }

    public string Information
    {
      get { return url; }
    }

    public string Url
    {
      get { return url; }
    }
       
    public string UserName
    {
      get { return userName; }
    }

    public string Password
    {
      get { return password; }
    }
          
    public string FileName
    {
      get { return fileName; }
    }

    public string FileFormat
    {
      get { return fileFormat; }
    }

    public bool OpenItemInBrowser
    {
      get { return openItemInBrowser; }
    }
    
    public int LastProjectID
    {
      get { return lastProjectID; }
    }

    public ItemType LastItemType
    {
      get { return lastItemType; }
    }

    public int LastItemID
    {
      get { return lastItemID; }
    }

  }
}
