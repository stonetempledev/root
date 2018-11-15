using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace deeper.lib
{

    public class xmlValidator
    {
        private bool _isValidXml = true;
        public bool IsValidXml { get { return _isValidXml; } }

        private string _validationError = "";
        public string validationError { get { return _validationError; } }

        public xmlValidator()
        {

        }

        private void callBack(object sender, ValidationEventArgs args)
        {
            _isValidXml = false;
            _validationError = args.Message + " (line: " + args.Exception.LineNumber.ToString() + ", col: " + args.Exception.LinePosition.ToString() + ")";
        }

        public bool valid(string xml, string schemaNamespace, string schemaUri)
        {
            try
            {
                if (xml == null || xml.Length < 1)
                    return false;

                return valid(new StringReader(xml), schemaNamespace, schemaUri);
            }
            catch (Exception ex)
            {
                _validationError = ex.Message;
                return false;
            }
        }

        public bool valid(XmlDocument xml, string schemaNamespace, string schemaUri)
        {
            try
            {
                if (xml == null)
                    return false;

                StringWriter sw = new StringWriter();
                XmlTextWriter xw = new XmlTextWriter(sw);
                xml.WriteTo(xw);

                StringReader srXml = new StringReader(sw.ToString());

                return valid(srXml, schemaNamespace, schemaUri);
            }
            catch (Exception ex)
            {
                _validationError = ex.Message;
                return false;
            }
        }

        public bool valid(StringReader xml, string schemaNamespace, string schemaUri)
        {
            if (xml == null || schemaNamespace == null || schemaUri == null)
                return false;

            _isValidXml = true;
            XmlValidatingReader vr;
            XmlTextReader tr;

            try
            {
                tr = new XmlTextReader(xml);
                vr = new XmlValidatingReader(tr);
                vr.ValidationType = ValidationType.Auto;

                XmlSchemaCollection schemaCol = new XmlSchemaCollection();
                schemaCol.Add(schemaNamespace, schemaUri);
                if (schemaCol != null)
                    vr.Schemas.Add(schemaCol);

                vr.ValidationEventHandler += new ValidationEventHandler(callBack);

                while (vr.Read()) { }

                vr.Close();

                return _isValidXml;
            }
            catch (Exception ex)
            {
                _validationError = ex.Message;
                return false;
            }
            finally
            {
                vr = null;
                tr = null;
            }
        }

        static public XmlSchemaElement findElement(XmlSchema schema, string xpath)
        {
            string[] xparts = xmlDoc.xpathParts(xpath);
            string name = xmlDoc.nameElement(xparts[1]);

            foreach (XmlSchemaObject item in schema.Items)
            {
                if (item is XmlSchemaElement && ((XmlSchemaElement)item).Name == name)
                {
                    try
                    {
                        return findElement((XmlSchemaElement)item, xparts, 2);
                    }
                    catch (Exception ex) { throw new Exception("non cè corrispondenza fra il percorso '" + xpath + "' e il relativo schema xml: '" + ex.Message + "'"); }
                }
            }

            throw new Exception("non cè corrispondenza fra il percorso '" + xpath + "' e il relativo schema xml");
        }

        static protected XmlSchemaElement findElement(XmlSchemaElement parent, string[] elements, int iElement)
        {
            if (iElement >= elements.Length)
                return parent;

            string name = xmlDoc.nameElement(elements[iElement]);
            XmlSchemaComplexType complexType = parent.SchemaType as XmlSchemaComplexType;
            foreach (XmlSchemaObject child in ((XmlSchemaSequence)complexType.Particle).Items)
            {
                if (child is XmlSchemaElement && ((XmlSchemaElement)child).Name == name)
                    return findElement((XmlSchemaElement)child, elements, iElement + 1);
            }

            throw new Exception("elemento '" + name + "' non trovato nel parent '" + parent.Name + "'");
        }
    }
}
