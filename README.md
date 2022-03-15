# HdHomerun Dotnet Library and Front-End

I started this project because I wanted to have more control over my HDHomerun's DVR service. My HDHomerun couldn't detect my 5 TB external WD drive so I had to use a 120 GB drive.  I found my recordings were stacking up and not being deleted. I was looking for a way to keep 3 episodes and delete the rest. Since I coudln't find anything like this using the DVR software, I decided to write my own library.

I wrote a front-end application that uses this library and it allows me to manage my recording pretty easily.  

There are still a few things I would like to do with libary.

1. Add the ability to run the front-end with a command passed in on the command line -- Done
2. Write a Windows service that can run automatically and keep your DVR recordings clean
3. Protect recordings from automatic removal -- Done
4. Show the newest recordings -- Done

# Running the Front-End

The front-end was a result of me consuming this library.  It works despite me wanting to clean-up and better document the code.  I encourage people to give it a try and let me know if you encounter any issues.  

The first thing you should do when you run it is to put it in simulation mode.  At the prompt, type sim.  In this mode, nothing will be deleted.  You'll see sim displayed on the prompt.  Type ? or help at the prompt to get an idea of the commands you can use.

