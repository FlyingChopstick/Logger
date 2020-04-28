# README #

### What is this repository for? ###

This is a custom logger library, used in my other project.
It provides basic text messages with customizable prefixes to relay importance of the message and/or to group messages.

### How do I get set up? ###

You have to use one of the variants of Initialize to set up the logger.
There are 6 overloads of that function, allowing for flexible setup. If initializing without providing logFolder path, logs will be saved into .\Logs.

It is best to do so in the beginning of the main function, so that you can safely log things from other parts of the program.
If you want to create footer, add Logger.End() near the end of the main.

You can use launch arguments array to initialize console logging (-ToHconsole), instead of the hard-coded way.

### What you can do with it? ###

My logger allows user to log different messages with possibility of type tag to indicate the meaning of the message.
Example (2020-04-28) - console and .txt log:
![Example (2020-04-28)](https://i.imgur.com/GxsPUUu.png "Example (2020-04-28)")

.txt files are rotated with maximum of three log files - current and two previous launches named log1.txt and log2.txt.
__Warning:__ rotation is quite picky as of now, and if it can't find log.txt, it will delete all files with "log" in the name to self-stabilize.

For now there are two ways to log something:
<details>
  <summary>Typeless logging</summary>
  <p>Logs single or an array of General messages (string or string[]) without the need of type specification:</p>
</details>
<details>
  <summary>Typed logging</summary>
  <p>Logs single or an array of messages with specified MessageType</p>
</details>

Message type is selected via MessageType enum, which consist of the following options:
* General message - no default prefix, white text
* General Submessage: " |" default prefix, white text
* Alert: "!" default prefix, yellow text
* Alert submessage: "! |" default prefix, yellow text
* High Alert: "!!" default prefix, red text
* High Alert submessage: "!! |" default prefix, red text
* Maintenance message: "~ " default prefix, dark gray text
* Maintenance submessage: "~ |" default prefix, dark gray text

Both prefixes and console text colors are customizable in source code (for now) in Prefixes and Highlights dictionaries respectively.

You can also queue messages to then log them all at once - use QueueAdd() and QueueExecute().
There are two text dividers - simple empty line and dashed (-------) line, which can be used to structurise the log.
