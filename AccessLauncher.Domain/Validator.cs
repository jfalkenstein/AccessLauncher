using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace AccessLauncher.Domain
{
    public class Validator
    {
        /*The Validator class exists to validate user input to ensure it is in an acceptable format.
         * 
         * All methods in this class are static. See descriptions on each method for explanation.
         */
        
        //This will verify that an input directory path exists --if not it will throw an exception. Then it will
        //ensure the directory path has a "\" on the end of it.
        public static string ValidateDirectoryPath(string PathToValidate)
        {
            if (!Directory.Exists(PathToValidate)) throw new FileNotFoundException("This folder could not be found.");
            if (PathToValidate[PathToValidate.Length - 1] != '\\') PathToValidate += "\\";
            return PathToValidate;
        }

        //This will return true if both the BackEndSettings.xml file exists AND the backend.xml file exists.
        //Otherwise, it will return false.
        public static bool BackEndIsInstalled()
        {
            bool value = false;
            if (BackEndSettingsFileExists())
            {
                if (BackEndXmlFileExists(DTO.Enums.BackEndOrFrontEndEnum.BackEnd))
                {
                     value = true;
                }
            }
            return value;
        }

        //See method name.
        public static bool BackEndSettingsFileExists()
        {
            if(File.Exists(PathManager.GetSettingsXMLPath(DTO.Enums.BackEndOrFrontEndEnum.BackEnd)))
            {
                return true;
            }
            else return false;
        }
        
        //See method name.
        public static bool BackEndXmlFileExists(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            if (File.Exists(PathManager.GetBackEndXmlPath(whichEnd)))
            {
                return true;
            }
            else return false;
        }

    }
}
