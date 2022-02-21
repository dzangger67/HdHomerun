# HdHomerun Dotnet Library and Front-End

I started this project because I wanted to have more control over my HDHomerun's DVR service. My HDHomerun couldn't detect my 5 TB external WD drive so I had to plug in a 120 GB drive.  And I found my recordings were stacking up and not being deleted. I was looking for a way to say only keep 3 episodes and delete the rest. Since I coudln't find anything like this using their DVR software, I decided to write my own library.

I wrote a front-end application that uses this library and it will allow you to manage your recording pretty easily.  

There are still a few things I would like to do with libary.

1. Add the ability to run the front-end with a command passed in on the command line
2. Write a Windows service that can run automatically and keep your DVR recordings clean
3. Protect recordings from automatic removal

# Running the Front-End

The front-end was attempt to consume my library.  I still have some things I'd like to clean-up.  But it does work.  I encourage people to use it and report any issues to me.  

The first thing you should do when you run it is to put it in simulation mode.  At the prompt, type sim.  You'll see sim displayed on the prompt.  Type ? or help at the prompt to get an idea of the commands you can use.

