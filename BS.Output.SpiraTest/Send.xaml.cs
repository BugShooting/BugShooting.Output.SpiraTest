using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace BS.Output.SpiraTest
{
  partial class Send : Window
  {
 
    public Send(string url, int lastProjectID, int lastItemType, int lastItemID,  string fileName)
    {
      InitializeComponent();
      
      // TODO
      //ProjectComboBox.ItemsSource = projectItems;



      Url.Text = url;
      ProjectComboBox.SelectedValue = lastProjectID;
      ItemTypeComboBox.SelectedValue = lastItemType;
      ItemIDTextBox.Text = lastItemID.ToString();
      FileNameTextBox.Text = fileName;

      ProjectComboBox.SelectionChanged += ValidateData;
      ItemTypeComboBox.SelectionChanged += ValidateData;
      ItemIDTextBox.TextChanged += ValidateData;
      CommentTextBox.TextChanged += ValidateData;
      FileNameTextBox.TextChanged += ValidateData;
      ValidateData(null, null);

    }

    public int ProjectID
    {
      get { return (int)ProjectComboBox.SelectedValue; }
    }

    public int ItemType
    {
      get { return (int)ItemTypeComboBox.SelectedValue; }
    }

    public string ItemID
    {
      get { return ItemIDTextBox.Text; }
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
                     Validation.IsValid(CommentTextBox) &&
                     Validation.IsValid(FileNameTextBox);
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

  }

}
