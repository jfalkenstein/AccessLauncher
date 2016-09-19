This is the documentation for the Access Launcher application, developed by 
Jonathan Falkenstein for Pensions and Benefits USA.

Basic Instructions:

1. To implement the Launcher, first you will need to run the AccessLauncher.BackEnd.exe
program. It will have you set up the rollout directory. This is the place that you want all
access files rollout out to. All front end launchers will look to this location for updates.

2. For each user you would like to install the launcher, have them run the AccessLauncher.
FrontEndInstaller.exe file. This can be run from the shortcut within the rollout directory. A link to this file
can be sent via email or by any other means.

3. Before users can use the launcher, you need to roll out at least one implementation of each
user type (currently Admin and Associate). Once these are rolled out, users' launchers will automatically
keep their front ends on the current version.


NOTE: IF THE LOCATION OF THE ACCESS PROJECT MANAGER DATABASE CHANGES, THAT INFO NEEDS TO BE MODIFIED IN THE 
"AccessLauncher.AccessDBInterface.dll.config" FILE. ALL USERS WILL HAVE TO REINSTALL AFTER THIS.

Layer/Module Documentation:
This Solution consists if 8 different layers/modules.

AccessLauncher.BackEnd: This is a WPF application used to manage rollouts and rollout settings. This application mostly
	implements code from the Domain layer.

AccessLauncher.FrontEnd: This is a console application used to launch the rolled out access files. This application mostly
	implements code from the Domain layer.

AccessLauncher.Domain: This is the central class library that the various user interfaces use as support.
	-This contains the central code for back end and front end operation. It also serves as a gatekeeper for
		data access to XML and the AccessDB.
	-This layer also contains the key operating code for the BackEnd, FrontEnd, and FrontEndInstaller. All three
		executables require the Domain layer to operate.

AccessLauncher.DTO: This layer is a class library, which functions as the "common language" used to pass objects 3
		and information between layers.
	-There are two Key Enums: BackEndOrFrontEndEnum and UserTypeEnum.
	-There are 15 custom exceptions located here that are used throughout the application.
	-There are three key classes: InstallerItem, RolloutInfo, and User.

AccessLauncher.XML: This module is a class library, dediated exclusively to reading and writing XML.
	-This layer has 7 classes, all full of static methods: Caching, CreateXmlDoc, GetData, GetPath, GetXmlDoc, SaveXmlDoc, and UpdateData

AccessLauncher.AccessDBInterface: This module is a class library that uses a dataset model to interact with the User_Login_Info
		table in the Project Manager database and obtain a list of users.
	-There is only one class in this layer, the UserInfoRepository, which solely functions to obtain a list of users.

AccessLauncher.FrontEndInstaller: This module is a console application that installs the user's front end. It will also update the user's
		front end when new launcher versions are pushed out.

AccessLauncher.FeUninstaller: This module is a simple console application that uninstalls the user's front end if they have been
		deauthorized.

New features added in March 2016 Update:
	1. Interaction with the Access DB is now controled through dependency injection, inverting the references between AccessDBInterface and
		the domain layer. This should allow the code to be more flexible. If we ever wanted to store user info in a different way than in an
		Access table, the domain layer wouldn't need to be modified. This dependency injection is done manually, making use of the
		IUserInfoRepository interface.
	2. The code has been reorganized and refactored to eliminate redundancy and make the purpose and grouping of methods more apparent.
		Instead of a singular XmlManager class, there are 4 new classes in the Domain layer: PathManager (for access to all the various filepaths),
		XmlAccess/CreateXmlFile (for Xml file creation), XmlAccess/GetDataFromXml (for obtaining data from the various xml files), and
		XmlAccess/UpdateXmlFile (for writing data to the various xml files). Also, instead of the three classes in the XML layer (Caching, GetData, and SetData),
		there are now 7 classes that similarly organize the code: Caching (same purpose), CreateXmlDoc (for creating XML files), GetData (for obtaining data from the
		Xml files), GetPath (for obtaining the various important file paths from the xml docs), GetXmlDoc (these methods return the various XML docs to be accessed),
		SaveXMLDoc (this is the singular path for saving all XML documents), and UpdateData (for writing into XmlDocuments);
	3.	The ability to change the path to the Access Back End.
	4.	To make all network paths use UNCs rather than mapped drive letters.
	5.	I implemented a menu on the back end tool to replace the multitude of buttons that were forming
		

