# bashgpt – ask ChatGPT to write bash scripts from your terminal

## Install

bashgpt relies on OpenAI's ChatGPT API, so you will need an account there (not ChatGPT+, but one that gives you access to the API). OpenAI gives you some free token budget when you sign up. Once it's exhausted, the [price](https://openai.com/pricing) is $0.002 for 1k tokens, as of 03/12/2023. bashgpt is limited to 512 tokens per script, and shorter scripts consume much less.

1. Install [dotnet 7.0+](https://dotnet.microsoft.com/en-us/download)
2. Clone this repository: `git clone https://github.com/dtegunov/bashgpt`
3. Inside the bashgpt directory, build: `dotnet publish /p:PublishProfile=FolderProfile --configuration Release`
4. Add the bin directory to your PATH: `export PATH=$PATH:/path/to/bashgpt/bin` 
(add this line to ~/.bashrc to have it included every time you open a new terminal)
5. Save your [OpenAI API key](https://help.openai.com/en/articles/4936850-where-do-i-find-my-secret-api-key) in ~/openai.key
6. The manual editing functionality requires `nano` to be installed

## Use
Just type `bashgpt`, followed by your request, and press enter. Once a script is generated, you can run it, edit manually or with ChatGPT's help, save, or cancel. When you save the script, a fitting name is auto-suggested by ChatGPT.

![Example of using bashgpt](https://github.com/dtegunov/bashgpt/blob/main/screenshot.png?raw=true "Example of using bashgpt")
