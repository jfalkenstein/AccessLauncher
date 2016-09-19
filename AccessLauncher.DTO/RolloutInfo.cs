using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessLauncher.DTO
{
    /* This is main class object used to carry information between layers. It has no methods,
     * only properties. This class makes it easier to work with the various information pieces
     * necessary to do the various functions of this application.
     * 
     * There is some special behavior when setting some of the properties on this class:
     * 
     * UserTypeName and UserType: UserTypeName is a string representation of the enumerated UserType.
     *  -If you enter the UserTypeName, it will automatically parse and set the UserType enum.
     *  -If you enter the UserType enum, it will automatically add the UserTypeName as a string.
     *  
     * The same behavior occurs regarding the RolloutVersionNumber and RolloutVersionString. Whichever is input, the
     * other will also be set.
     * 
     * This allows for easy conversion to and from strings, which is necessary when working with XML documents.
     */
    
    public class RolloutInfo
    {
        public RolloutInfo()
        {
            this.Connection = new ConnectionInfo();
        }
        
        private string _userTypeName;

        public string UserTypeName
        {
            get {return _userTypeName; }
            set 
            { 
                _userTypeName = value;
                _userType = (DTO.Enums.UserTypeEnum)Enum.Parse(typeof(DTO.Enums.UserTypeEnum), value);
            }
        }
        private int _rolloutVersionNumber;

        public int RolloutVersionNumber
        {
            get { return _rolloutVersionNumber; }
            set 
            { 
                _rolloutVersionNumber = value;
                _rolloutVersionString = value.ToString();
            }
        }
        private string _rolloutVersionString;

        public string RolloutVersionString
        {
            get { return _rolloutVersionString; }
            set 
            { 
                _rolloutVersionString = value;
                _rolloutVersionNumber = int.Parse(value);
            }
        }
        
        public string LaunchFile { get; set; }
        public string ZipPath { get; set; }
        public string RolloutDirectory { get; set; }
        public DateTime DateTimeStamp { get; set; }
        private DTO.Enums.UserTypeEnum _userType;

        public DTO.Enums.UserTypeEnum UserType
        {
            get { return _userType; }
            set 
            { 
                _userType = value;
                _userTypeName = Enum.GetName(typeof(DTO.Enums.UserTypeEnum), value);
            }
        }
        public string UninstallerPath { get; set; }
        public ConnectionInfo Connection { get; set; }

        public class ConnectionInfo
        {
            public string ConnectionString { get; set; }
            public DateTime? DateSet { get; set; }
        }

    }
}
