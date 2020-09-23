using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;


public partial class _Default : Page
{
    struct ios
    {
        public string ID;
        public string Name;
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        DateTime Transaction_Date = DateTime.ParseExact("20/02/2019 12:33:16", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
    }
    public void btnsave_Click(object sender, EventArgs e)
    {
        StringBuilder sb = new StringBuilder();
        operationDB op = new operationDB();
        List<TransectionDB> ltrans;
        if (FileUpload1.HasFile)
        {
            try
            {
                sb.AppendFormat(" Uploading file: {0}", FileUpload1.FileName);
                string fn = System.IO.Path.GetFileName(FileUpload1.PostedFile.FileName);
                string SaveLocation = Server.MapPath("Data") + "\\" + fn;
                sb.AppendFormat("<br/> Save As: {0}", FileUpload1.PostedFile.FileName);
                sb.AppendFormat("<br/> File type: {0}", FileUpload1.PostedFile.ContentType);
                sb.AppendFormat("<br/> File length: {0}", FileUpload1.PostedFile.ContentLength);
                sb.AppendFormat("<br/> File name: {0}", FileUpload1.PostedFile.FileName);
                if (ValidationFile(FileUpload1)&&ValidationFormat(SaveLocation,out ltrans))
                {
                    FileUpload1.SaveAs(SaveLocation);
                    foreach (var lt in ltrans)
                    {
                        if (op.InsertTransection(lt))
                        {
                            Response.Write(ValidationMessage);
                        }
                        else
                        {
                            Response.Write(op.OperationMessage);
                        }
                    }
                }
                else
                {
                    Response.Write(ValidationMessage);

                }
            }
            catch (Exception ex)
            {
                Response.Write("Error: " + ex.Message);
            }
        }
        else
        {
            lblmessage.Text = sb.ToString();
        }
    }
    private string ValidationMessage { get; set; }
    private decimal filesize { get; set; }
    private bool ValidationFile(FileUpload file)
    {


        try
        {
            var supportedTypes = new[] { "csv", "xml" };
            var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
            if (!supportedTypes.Contains(fileExt))
            {
                ValidationMessage = "File Is InValid - Only support both formats csv and xml.";
                return false;
            }
            else
            {
                ValidationMessage = "File Is Successfully Uploaded";
                return true;
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = "Unknown format.";
            return false;
        }
    }
    
    
    private bool ValidationFormat(string file , out List<TransectionDB> ltrans)
    {

        try
        {
            var supportedTypes = new[] { "csv", "xml" };
            var fileExt = System.IO.Path.GetExtension(file).Substring(1);
            if (fileExt=="csv")
            {
                var r = ReadCsvFile(file);
                if (r.Count() > 0)
                {
                    ltrans = r;
                    return true;
                }
                else
                {
                    ltrans = new List<TransectionDB>();
                    return false;
                }
            }
            else
            {
                var doc = XDocument.Load(file);
                IEnumerable<TransectionDB> result = from rec in doc.Descendants("Transaction")
                                                    select new TransectionDB()
                                                    {
                                                        Id = Guid.NewGuid(),
                                                        Transaction_Date = DateTime.ParseExact(rec.Element("TransactionDate").Value.ToString().Replace("T", " "), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                                                        Transaction_Id = (string)rec.Attribute("id"),
                                                        Amount = (decimal)rec.Element("PaymentDetails").Element("Amount"),
                                                        Currency_Code = (string)rec.Element("PaymentDetails").Element("CurrencyCode"),
                                                        Status = (string)rec.Element("Status")
                                                    };
                ltrans = result.ToList<TransectionDB>();
                return true;
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = "UnValidation format."+ ex.Message;
            ltrans = new List<TransectionDB>();
            return false;
        }
    }
    public List<TransectionDB>  ReadCsvFile(string file)
    {
        try
        {
            List<TransectionDB> tdbL = new List<TransectionDB>();
            string Fulltext;
            if (FileUpload1.HasFile)
            {

                using (StreamReader sr = new StreamReader(file))
                {
                    while (!sr.EndOfStream)
                    {
                        Fulltext = sr.ReadToEnd().ToString();
                        string[] rows = Fulltext.Split('\n');
                        for (int i = 0; i < rows.Count() - 1; i++)
                        {
                            if (rows[i] != "")
                            {
                                string[] rowValues = rows[i].Replace("\r", "").Split(',');
                                {
                                    TransectionDB tdb = new TransectionDB();
                                    tdb.Id = Guid.NewGuid();
                                    tdb.Transaction_Id = rowValues[0].ToString();
                                    tdb.Amount = decimal.Parse(rowValues[1].ToString());
                                    tdb.Currency_Code = rowValues[2].ToString();
                                    tdb.Transaction_Date= DateTime.ParseExact(rowValues[3].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                    tdb.Status = rowValues[4].ToString();
                                    tdbL.Add(tdb); //add other rows  

                                }
                            }
                        }
                    }
                }
            }
            return tdbL;
        }
        catch(Exception ex)
        {
            ValidationMessage = "UnValidation format." + ex.Message;
            return null;
        }
    }


}
