using BugShooting.Output.SpiraTest.SoapService;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace BugShooting.Output.SpiraTest
{
  partial class Send : Window
  {
 
    public Send(string url, int lastProjectID, ItemType lastItemType, int lastItemID, RemoteProject[] projects, string fileName)
    {
      InitializeComponent();

      ProjectComboBox.ItemsSource = projects;
      
      List<ItemTypeItem> itemTypes = new List<ItemTypeItem>();
      itemTypes.Add(new ItemTypeItem(ItemType.Incident, "Incident"));
      itemTypes.Add(new ItemTypeItem(ItemType.Task, "Task"));
      itemTypes.Add(new ItemTypeItem(ItemType.Requirement, "Requirement"));
      itemTypes.Add(new ItemTypeItem(ItemType.Release, "Release"));
      itemTypes.Add(new ItemTypeItem(ItemType.TestCase, "Test Case"));
      itemTypes.Add(new ItemTypeItem(ItemType.TestSet, "Test Set"));
      itemTypes.Add(new ItemTypeItem(ItemType.TestStep, "Test Step"));
      itemTypes.Add(new ItemTypeItem(ItemType.TestRun, "Test Run"));
      ItemTypeComboBox.ItemsSource = itemTypes;

      Url.Text = url;
      ProjectComboBox.SelectedValue = lastProjectID;
      ItemTypeComboBox.SelectedValue = lastItemType;
      ItemIDTextBox.Text = lastItemID.ToString();
      FileNameTextBox.Text = fileName;

      ProjectComboBox.SelectionChanged += ValidateData;
      ItemTypeComboBox.SelectionChanged += ValidateData;
      ItemIDTextBox.TextChanged += ValidateData;
      FileNameTextBox.TextChanged += ValidateData;
      ValidateData(null, null);

    }

    public int ProjectID
    {
      get { return (int)ProjectComboBox.SelectedValue; }
    }

    public ItemType ItemType
    {
      get { return (ItemType)ItemTypeComboBox.SelectedValue; }
    }

    public int ItemID
    {
      get { return Convert.ToInt32(ItemIDTextBox.Text); }
    }

    public string Comment
    {
      get { return CommentTextBox.Text; }
    }
    
    public string FileName
    {
      get { return FileNameTextBox.Text; }
    }

    private void ItemID_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
    }
    
    private void ValidateData(object sender, EventArgs e)
    {
      OK.IsEnabled = Validation.IsValid(ProjectComboBox) && 
                     Validation.IsValid(ItemTypeComboBox) && 
                     Validation.IsValid(ItemIDTextBox) &&
                     Validation.IsValid(FileNameTextBox);
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

  }

  internal class ItemTypeItem
  {

    ItemType itemType;
    string name;

    public ItemTypeItem(ItemType itemType, string name)
    {
      this.itemType = itemType;
      this.name = name;
    }

    public ItemType ItemType
    {
      get { return itemType; }
    }

    public string Name
    {
      get { return name; }
    }

  }

}
