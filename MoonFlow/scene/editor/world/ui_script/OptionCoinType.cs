using Godot;
using System.Collections.Generic;
using System;
using System.Linq;

public partial class OptionCoinType : OptionButton
{
	public static readonly Dictionary<char, Texture2D> Table = new(){
        {'A', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_50.png")},
        {'B', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_51.png")},
        {'C', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_52.png")},
        {'D', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_53.png")},
        {'N', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_54.png")},
        {'F', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_55.png")},
        {'E', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_56.png")},
        {'H', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_57.png")},
        {'I', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_58.png")},
        {'G', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_59.png")},
        {'J', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_5A.png")},
        {'K', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_5B.png")},
        {'L', GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_5C.png")},
    };

	[Export]
	public LineEdit CustomInput;

	[Signal]
    public delegate void CoinCollectSelectedEventHandler(string id);
	
	public override void _Ready()
	{
		foreach (var item in Table)
			AddIconItem(item.Value, item.Key.ToString());
		
		AddItem("Custom ID");
	}

	public void OnSelection(int id)
	{
		if (id == ItemCount - 1)
		{
			CustomInput.Show();
			return;
		}

		CustomInput.Hide();
		EmitSignal(SignalName.CoinCollectSelected, Table.Keys.ElementAt(id).ToString());
	}
	
	public void SetSelection(string key)
	{
		var c = key.ElementAt(0);
		
		var idx = Table.Keys.ToList().FindIndex((s) => s.Equals(c));
		if (idx == -1)
		{
			Selected = ItemCount - 1;
			CustomInput.Show();
			return;
		}
		
		Selected = idx;
		CustomInput.Hide();
	}
}
