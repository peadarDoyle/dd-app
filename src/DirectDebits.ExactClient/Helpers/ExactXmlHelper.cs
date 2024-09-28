using System;
using System.Linq;
using System.Xml;
using System.IO;
using System.Xml.Schema;
using System.Text;
using System.Collections.Generic;
using System.Xml.Linq;
using DirectDebits.Common.Utility;

namespace DirectDebits.ExactClient.Helpers
{
    public abstract class ExactXmlHelper
    {
        public static Result<XDocument> ValidateFormat(string response)
        {
            XDocument document;

            bool isValid;

            try
            {
                document = XDocument.Parse(response);
                string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
                string exactXsd = basePath + @"\Validation\eExact-XML.xsd";
                isValid = IsValidForSchema(document, exactXsd);
            }
            catch (XmlException)
            {
                string msg = "The Exact Online reponse is not valid XML";
                return Result.Fail<XDocument>(msg);
            }
            catch (FileNotFoundException)
            {
                string msg = "The Exact Online reponse could not be processed because the schema document could not be found";
                return Result.Fail<XDocument>(msg);
            }

            return isValid ? Result.Ok(document) : Result.Fail<XDocument>("The Exact Online response is not valid as per the current schema");
        }

        public static Result ValidateContent(XDocument document)
        {
           // error codes are defined in the XSD for the Exact Online XML data
           var errorCodes = new []
            {
                "0", // error
                "1", // warning
                "3"  //  fatal error
            };

            IList<string> errors =  document.Descendants("Messages").Elements("Message")
                                            .Where(x => errorCodes.Contains(x.Attribute("type").Value))
                                            .Select(x =>x.Element("Description").Value)
                                            .ToList();

            switch (errors.Count())
            {
                case 0:
                    return Result.Ok();
                case 1:
                    return Result.Fail("An error occurred uploading information to Exact Online: " + errors.Single());
                default:
                    var sb = new StringBuilder();
                    sb.Append("Multiple errors occurred while uploading information to Exact Online.");

                    for (int i = 0; i < errors.Count(); i++)
                    {
                        string error = $" Error {i+1}: {errors[i]}";
                        sb.Append(error);
                    }

                    return Result.Fail(sb.ToString());
            }
        }

        public static bool IsValidForSchema(XDocument xdoc, string xsdFilePath)
        {
            var schemas = new XmlSchemaSet();
            schemas.Add(null, xsdFilePath);

            try
            {
                xdoc.Validate(schemas, null);
            }
            catch (XmlSchemaValidationException)
            {
                return false;
            }

            return true;
        }

    }
}