# YoutubeSyn
Synced youtube playlist between clients.

#MongoDB setup
Installing Mongo on Windows

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

Check if the service is active by opening the Run menu (Windows + R) and typing services.msc.
