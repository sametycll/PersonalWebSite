namespace SyweachWeb.ViewModels
{
    public class ProjectViewModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? ProjectUrl { get; set; }
        public List<string>? Technologies { get; set; }
    }

    public class SkillViewModel
    {
        public string? Name { get; set; }
        public string? Icon { get; set; } // Lucide or FontAwesome icon name
        public string? Category { get; set; }
    }

    public class HomeViewModel
    {
        public List<ProjectViewModel>? Projects { get; set; }
        public List<SkillViewModel>? Skills { get; set; }
        public string? Version { get; set; }
    }
}
