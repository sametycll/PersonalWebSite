using SyweachWeb.ViewModels;

namespace SyweachWeb.Services
{
    public interface IDataService
    {
        List<ProjectViewModel> GetProjects();
        List<SkillViewModel> GetSkills();
        string GetVersion();
    }

    public class MockDataService : IDataService
    {
        public List<ProjectViewModel> GetProjects()
        {
            return new List<ProjectViewModel>
            {
                new ProjectViewModel
                {
                    Title = "Personal Library App",
                    Description = "A cross-platform mobile app for managing physical libraries with ISBN scanning.",
                    ImageUrl = "/images/projects/library.jpg",
                    ProjectUrl = "https://github.com/sametycll",
                    Technologies = new List<string> { "Flutter", "SQLite", "Dart" }
                },
                new ProjectViewModel
                {
                    Title = "KuranWeb",
                    Description = "A spiritual web platform for searching verses and hadiths based on themes.",
                    ImageUrl = "/images/projects/kuranweb.jpg",
                    ProjectUrl = "https://github.com/sametycll",
                    Technologies = new List<string> { "ASP.NET Core", "Entity Framework", "C#" }
                }
            };
        }

        public List<SkillViewModel> GetSkills()
        {
            return new List<SkillViewModel>
            {
                new SkillViewModel { Name = "C# / .NET 8", Icon = "devicon-csharp-plain", Category = "Backend" },
                new SkillViewModel { Name = "ASP.NET Core MVC/WebAPI", Icon = "devicon-dotnetcore-plain", Category = "Backend" },
                new SkillViewModel { Name = "Flutter / Dart", Icon = "devicon-flutter-plain", Category = "Mobile" },
                new SkillViewModel { Name = "MS SQL Server / PostgreSQL", Icon = "devicon-microsoftsqlserver-plain", Category = "Database" },
                new SkillViewModel { Name = "Tailwind CSS", Icon = "devicon-tailwindcss-plain", Category = "Frontend" },
                new SkillViewModel { Name = "JavaScript / jQuery", Icon = "devicon-javascript-plain", Category = "Frontend" },
                new SkillViewModel { Name = "AI / Prompt Engineering", Icon = "lucide-brain", Category = "AI" }
            };
        }

        public string GetVersion() => "v.1.0.0";
    }
}
