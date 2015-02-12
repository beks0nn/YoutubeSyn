# YoutubeSyn
Synced youtube playlist between clients.

#MongoDB setup
Installing Mongo on Windows

I had a very difficult time getting MongoDB to install and run properly without using the Administrator account. This isn’t a requirement with the “Run as Administrator” option is always available.

But if you have the ability, just run this in command prompt and then restart your computer. You should notice the Administrator as a new login selection.

    net user administrator /active:yes  

If you run into trouble, MongoDB has a great online install guide created specifically for Windows users. To get your copy, visit the downloads page and find your version of Windows. As of the writing this article, the latest stable release is MongoDB 2.2.0: here are the win32 and win64 direct downloads.

We are going to place all these files directly inside a directory C:\mongodb\. So once the download finishes, extract the zip and open the folders until you find /bin/ with a few other files. Select all these and cut/paste into our new C:\mongodb\ directory.

Now still inside this folder alongside \bin\ create a new folder named “log” which will store all the MongoDB system logs. We also need to create two external directories for data storage, C:\data\ and C:\data\db\.

This is the part where using a non-Administrator account may cause trouble. Open up the command prompt and run cd C:\mongodb\bin.

Then we are looking to start the mongod.exe in shell, but after running this you’ll notice the operation will freeze when listening for connections. Well it’s not actually frozen, we are running Mongo directly through the terminal.

This will get annoying to do every time, so let’s run a command configuring Mongo to start automatically as a Windows Service.

    > echo logpath=C:\mongodb\log\mongo.log > C:\mongodb\mongod.cfg  

This first command will create a log file and configuration for the service. Not required, but good practice as a database administrator.

Now run these two lines of code in terminal to create the service and get it started.

    > C:\mongodb\bin\mongod.exe --config C:\mongodb\mongod.cfg --install  
    > net start MongoDB  

If you get no errors then the process is all done!

Check if the service is active by opening the Run menu (Windows + R) and typing services.msc.

This brings up an active list of services and if you scroll down you should find Mongo DB with the status “active” and the startup type “automatic”.

As on the Mac install you can access the Mongo shell terminal right from the command prompt. Change directories to C:\mongodb\bin\ and type mongo, then hit Enter.

You should gain access right into the current MongoDB server. Now you can run Mongo shell commands to create databases, collections, store new data, or edit old data entries.

Run the following line to show all current databases on the server:

    > show dbs  
