using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Awareness;
using PlayFabCLI.Services;
using PlayFabToolSDK.Models;
using PlayFabToolSDK.Services;

namespace PlayFabCLI.Commands
{

    /// <summary>
    /// This command generates 3 titles for the application and sets up migration configs to migrate from DEV to TEST and from TEST to LIVE
    /// </summary>
    public class GenerateAppAsyncConsoleCommand : ConsoleCommandAsync
    {

        private readonly IPlayFabEditorService _editorService;
        private readonly IMigrationConfigService _migrationConfigService;
        private readonly ILogger _logger;
        private string _username;
        private string _password;
        private string _titleName;
        private string _studioId;

        public GenerateAppAsyncConsoleCommand(IPlayFabEditorService editorService, ILogger logger, IMigrationConfigService migrationConfigService)
        {
            _editorService = editorService;
            _logger = logger;
            _migrationConfigService = migrationConfigService;
            IsCommand("generate-app", "Generates dev/test/live titles and creates corresponding migration configs");
            HasOption("u|username=", "Specify username to identify current user", u => _username = u);
            HasOption("p|password=", "Specify password to identify current user", p => _password = p);
            HasOption("n|name=", "Specify name for the app", p => _titleName = p);
            HasOption("s|studio=", "Specify studio to generate app titles in", p => _studioId = p);
        }

        public override async Task RunAsync(string[] remainingArguments)
        {
            //string username = null, password = null;

            while (string.IsNullOrEmpty(_username))
            {
                Console.Write("\nPublisher Username: ");
                _username = Console.ReadLine();
            }
            while (string.IsNullOrEmpty(_password))
            {
                Console.Write("\nnPublisher Password: ");
                _password = Console.ReadLine();
            }

            Console.Write("\nSigning in... ");

            var authResponse = await _editorService.LoginAsync(new LoginRequest()
            {
                Email = _username,
                Password = _password,
                DeveloperToolProductName = typeof(Program).GetAssembly().GetName().FullName,
                DeveloperToolProductVersion = typeof(Program).GetAssembly().GetName().Version.ToString(),
            });

            if (authResponse.Error != null)
            {
                _logger.Error("Failed to authenticate publisher!", this);
                _logger.Error(authResponse.Error.GenerateErrorReport(),this);
                return;
            }

            var authToken = authResponse.Result.DeveloperClientToken;

            Console.WriteLine($"Authenticated!");
            if (string.IsNullOrEmpty(_studioId))
            {
                Console.WriteLine($"Listing studios...");

                var studiosResponse = await _editorService.GetStudiosAsync(new GetStudiosRequest()
                {
                    DeveloperClientToken = authToken,
                });

                if (studiosResponse.Error != null)
                {
                    _logger.Error("Failed to list studios!", this);
                    _logger.Error(studiosResponse.Error.GenerateErrorReport(), this);
                    return;
                }

                var studios = studiosResponse.Result.Studios.ToArray();

                Console.WriteLine($"New titles can be generated for one of the following studios:");

                while (string.IsNullOrEmpty(_studioId))
                {
                    int studioIndex = -1;
                    for (int i = 0; i < studios.Length; i++)
                    {
                        Console.WriteLine($"\n({i + 1}) {studios[i].Name}");
                    }
                    Console.Write($"\nSelect Studio (1-{studios.Length}): ");
                    if (int.TryParse(Console.ReadLine().Trim(), out studioIndex))
                    {
                        _studioId = studios[studioIndex - 1].Id;
                    }
                }
            }

            while (string.IsNullOrEmpty(_titleName))
            {
                Console.Write("\nPlease, enter title name: ");
                _titleName = Console.ReadLine().Trim();
            }

            Console.Write("\nGenerating titles...");
            var devTitleName = _titleName + " Development";
            var testTitleName = _titleName + " Test";
            var prodTitleName = _titleName + " Live";

            var devTitleResponse = await _editorService.CreateTitleAsync(new CreateTitleRequest()
            {
                DeveloperClientToken = authToken,
                StudioId = _studioId,
                Name = devTitleName
            });

            if (devTitleResponse.Error != null)
            {
                _logger.Error("Failed to create title!", this);
                _logger.Error(devTitleResponse.Error.GenerateErrorReport(), this);
                return;
            }

            var testTitleResponse = await _editorService.CreateTitleAsync(new CreateTitleRequest()
            {
                DeveloperClientToken = authToken,
                StudioId = _studioId,
                Name = testTitleName
            });

            if (testTitleResponse.Error != null)
            {
                _logger.Error("Failed to create title!", this);
                _logger.Error(testTitleResponse.Error.GenerateErrorReport(), this);
                return;
            }

            var prodTitleResponse = await _editorService.CreateTitleAsync(new CreateTitleRequest()
            {
                DeveloperClientToken = authToken,
                StudioId = _studioId,
                Name = prodTitleName
            });

            if (prodTitleResponse.Error != null)
            {
                _logger.Error("Failed to create title!", this);
                _logger.Error(prodTitleResponse.Error.GenerateErrorReport(), this);
                return;
            }

            var devTitle = devTitleResponse.Result;
            var testTitle = testTitleResponse.Result;
            var prodTitle = prodTitleResponse.Result;

            Console.Write("\nGenerated the following titles: ");
            Console.Write($"\n  [{devTitle.Title.Id}] {devTitleName }");
            Console.Write($"\n  [{testTitle.Title.Id}] {testTitleName }");
            Console.Write($"\n  [{prodTitle.Title.Id}] {prodTitleName }");

            var devTitleReference = new TitleReference()
            {
                TitleId  = devTitle.Title.Id,
                DeveloperKey = devTitle.Title.SecretKey
            };
            var testTitleReference = new TitleReference()
            {
                TitleId = testTitle.Title.Id,
                DeveloperKey = testTitle.Title.SecretKey
            };
            var prodTitleReference = new TitleReference()
            {
                TitleId  = prodTitle.Title.Id,
                DeveloperKey = prodTitle.Title.SecretKey
            };

            Console.Write("\nGenerating migration configs...");

            var testConfig = await _migrationConfigService.GenerateMigrationConfig(devTitleReference, testTitleReference, new List<string>());
            await _migrationConfigService.SaveMigrationConfig("test", testConfig);
            var liveConfig = await _migrationConfigService.GenerateMigrationConfig(testTitleReference, prodTitleReference, new List<string>());
            await _migrationConfigService.SaveMigrationConfig("live", testConfig);

            Console.Write("\nComplete!");
        }

    }
}
