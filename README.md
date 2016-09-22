#AccessLauncher
An application to keep an entire department's Access databases synchronized.

##Purpose
This was my first real C# Project, mostly designed at the end of 2015 and the
beginning of 2016. At my work, there are a number of people that utilize
Access databases. There are a package of access databases. While they all share
some common data sources (and thus are linked to them over the network), they
Also each need some of tables and forms specific to their own workstation.

There are also two different sets of Access fils, Admin and Associate. This
is a solution that will use xml and a back end tool to "install" the access 
databases to the user's computer and then make sure that the user's current
implementation is up to date. We regularly have to push out updates to the 
tables, forms, vba modules, etc... Sometimes we also have to deactivate the
whole system for us to work on the back end. As a result, this application
keeps all users in sync, up to date, and allows new version and even new
dlls and other resourses to be pushed out reliably.

##Permissions
While this application was a product of my work, I have been given permission
by my supervisor to post this on GitHub for the purposes of developing my online
portfolio. All sensitive data has been scrubbed from it.