# README #

### What is this repository for? ###

This is a custom logger library, used in my other project.
It provides basic text messages with customizable prefixes to relay importance of the message and/or group messages

### How do I get set up? ###

You have to use one of the variants of Initialize to set up the logger.
It is best to do it in the beginning of the main function, so that you can safely log things from other parts of the program.
If you want to create footer, add Logger.End() near the end of the main.

You can use launch arguments array to initialize console logging, instead of the hard-coded way.