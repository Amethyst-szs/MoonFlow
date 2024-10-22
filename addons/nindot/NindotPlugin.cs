#if TOOLS
using Godot;
using Nindot;

[Tool]
public partial class NindotPlugin : EditorPlugin
{
	private SarcResourceImport PluginSarc;
	private BymlResourceImport PluginByml;
	private MsbtResourceImport PluginMsbt;

	public override void _EnterTree()
	{
		PluginSarc = new SarcResourceImport();
		AddImportPlugin(PluginSarc);

		PluginByml = new BymlResourceImport();
		AddImportPlugin(PluginByml);

		PluginMsbt = new MsbtResourceImport();
		AddImportPlugin(PluginMsbt);
	}

	public override void _ExitTree()
	{
		RemoveImportPlugin(PluginSarc);
		PluginSarc = null;

		RemoveImportPlugin(PluginByml);
		PluginByml = null;

		RemoveImportPlugin(PluginMsbt);
		PluginMsbt = null;
	}
}
#endif
