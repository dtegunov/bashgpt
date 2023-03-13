using System.Diagnostics;
using Mono.Unix.Native;
using OpenAI_API;

namespace bashgpt;

class Program
{

    static async Task Main(string[] args)
    {
        string HomeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string APIKey = File.ReadAllText(Path.Join(HomeDir, "openai.key")).Trim();

        var API = new OpenAIAPI(APIKey);

        API.Chat.DefaultChatRequestArgs.Model = OpenAI_API.Models.Model.ChatGPTTurbo;
        API.Chat.DefaultChatRequestArgs.Temperature = 0;
        API.Chat.DefaultChatRequestArgs.TopP = 1;
        API.Chat.DefaultChatRequestArgs.FrequencyPenalty = 0;
        API.Chat.DefaultChatRequestArgs.PresencePenalty = 0;
        API.Chat.DefaultChatRequestArgs.MaxTokens = 512;

        var Chat = API.Chat.CreateConversation();
        Chat.AppendSystemMessage("You are a world-class programmer and an expert in Unix bash scripting. " +
                                 "You write very clean code. " +
                                 "You are very careful. " +
                                 "Don't explain what the script does at the end. " +
                                 "If you have anything else to add at the end, include it as comments in the script. " +
                                 "Output only the script. " +
                                 "Be concise.");// but add comments to the code where needed.");

        Chat.AppendUserInput("Write a bash script that does the following: " +
                             DecorateMessage(string.Join(" ", args)));

        string FinalScript = "";
        {
            var Result = await Chat.GetResponseFromChatbot();
            FinalScript = CleanScript(Result);
        }

        while (true)
        {
            Console.WriteLine("");
            Console.WriteLine("Here is your script:");
            Console.WriteLine(FinalScript);
            Console.WriteLine("");

            PromptResult UserAction = ShowPrompt();

            if (UserAction == PromptResult.Run)
            {
                break;
            }
            else if (UserAction == PromptResult.Edit)
            {
                string TempName = Guid.NewGuid().ToString().Replace("-", "") + ".sh";
                WriteScriptFile(TempName, FinalScript);

                var BashProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = "nano",
                        CreateNoWindow = false,
                        Arguments = $"{TempName}"
                    }
                };
                BashProcess.Start();
                BashProcess.WaitForExit();

                FinalScript = File.ReadAllText(TempName);

                File.Delete(TempName);
                continue;
            }
            else if (UserAction == PromptResult.AskEdit)
            {
                Console.WriteLine("Enter request:");

                string EditRequest = Console.ReadLine() ?? "";
                if (string.IsNullOrEmpty(EditRequest))
                    continue;

                Chat.AppendUserInput(DecorateMessage(EditRequest));

                var Result = await Chat.GetResponseFromChatbot();
                FinalScript = CleanScript(Result);
                continue;
            }
            else if (UserAction == PromptResult.Save)
            {
                if (await SaveScript(Chat, FinalScript))
                {
                    Console.WriteLine("Script saved!");
                    return;
                }
                continue;
            }
            else if (UserAction == PromptResult.Cancel)
            {
                return;
            }
        }

        Console.WriteLine("Running script...");

        {
            string TempName = Guid.NewGuid().ToString().Replace("-", "") + ".sh";
            WriteScriptFile(TempName, FinalScript);

            var BashProcess = new Process
            {
                StartInfo =
                {
                    FileName = "bash",
                    CreateNoWindow = false,
                    Arguments = $"{TempName}"
                }
            };
            BashProcess.Start();
            BashProcess.WaitForExit();

            File.Delete(TempName);
        }

        while (true)
        {
            string Input = ReadLine.Read("Save script? (y)es / (n)o: ");

            if (string.IsNullOrEmpty(Input))
                continue;

            switch (Input.ToLower()[0])
            {
                case 'y':
                    await SaveScript(Chat, FinalScript);
                    return;
                case 'n':
                    return;
                default:
                    continue;
            }
        }
    }

    static string DecorateMessage(string message)
    {
        if (message.Last() != '.')
            message += '.';

        message += " Output only the script.";

        return message;
    }

    static string CleanScript(string script)
    {
        script = script.Trim();

        int ScriptStart = script.IndexOf("```") + 3;
        int ScriptEnd = script.LastIndexOf("```");

        if (ScriptEnd > ScriptStart)
            script = script.Substring(ScriptStart, ScriptEnd - ScriptStart);
        else
            script = script.Replace("```", "");

        if (script.IndexOf("#!/bin/bash") >= 0)
            script = script.Substring(script.IndexOf("#!/bin/bash"));

        return script;
    }

    static async Task<bool> SaveScript(OpenAI_API.Chat.Conversation chat, string script)
    {
        chat.AppendUserInput("What would be a good name for this script file? Output only the name.");

        string FileName = await chat.GetResponseFromChatbot();
        FileName = ReadLine.Read($"Enter file name (default: {FileName}): ", FileName);

        if (string.IsNullOrEmpty(FileName))
        {
            Console.WriteLine("No file name given");
            return false;
        }

        WriteScriptFile(FileName, script);

        return true;
    }

    static void WriteScriptFile(string path, string content)
    {
        File.WriteAllText(path, content);
        Syscall.chmod(path, FilePermissions.S_IRWXU | FilePermissions.S_IRGRP | FilePermissions.S_IROTH);
    }

    static PromptResult ShowPrompt()
    {
        while (true)
        {
            string Input = ReadLine.Read("(r)un, (e)dit, (a)sk GPT to edit, (s)ave, (c)ancel? ");

            if (string.IsNullOrEmpty(Input))
                continue;

            switch (Input.ToLower()[0])
            {
                case 'r':
                    return PromptResult.Run;
                case 'e':
                    return PromptResult.Edit;
                case 'a':
                    return PromptResult.AskEdit;
                case 's':
                    return PromptResult.Save;
                case 'c':
                    return PromptResult.Cancel;
                default:
                    continue;
            }
        }
    }

    public enum PromptResult
    {
        Run,
        Edit,
        AskEdit,
        Save,
        Cancel
    }
}

