using Godot;
using System;

public enum UpgradeRarity
{
	Uncommon,
	Rare,
	Epic,
	Legendary
}

public partial class Upgrade : Item
{
	[Export] public UpgradeRarity Rarity = UpgradeRarity.Uncommon;
	
	[Export(PropertyHint.Range, "0,1,0.01")] public float HpPercent = 0f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float DmgPercent = 0f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float AtkSpdPercent = 0f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float DefPercent = 0f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float SpdPercent = 0f;


	public static PackedScene UncommonAura;
	public static PackedScene RareAura;
	public static PackedScene EpicAura;
	public static PackedScene LegendaryAura;

	private static readonly string UncommonAuraPath = "res://Scenes/Items/UpgradeRarity/UncommonUpgrade.tscn";
	private static readonly string RareAuraPath = "res://Scenes/Items/UpgradeRarity/RareUpgrade.tscn";
	private static readonly string EpicAuraPath = "res://Scenes/Items/UpgradeRarity/EpicUpgrade.tscn";
	private static readonly string LegendaryAuraPath = "res://Scenes/Items/UpgradeRarity/LegendaryUpgrade.tscn";

	[Export] public AudioStreamPlayer2D CollectSound;

	public static UpgradeRarity GetRandomRarity()
	{
		float roll = (float)GD.RandRange(0f, 100f);
		
		if (roll < 55f)
			return UpgradeRarity.Uncommon;    // 0-55% (55%)
		else if (roll < 85f)
			return UpgradeRarity.Rare;       // 55-85% (30%)
		else if (roll < 95f)
			return UpgradeRarity.Epic;       // 85-95% (10%)
		else
			return UpgradeRarity.Legendary;  // 95-100% (5%)
	}


	public override void _Ready()
	{
		base._Ready();
		var area = GetNode<Area2D>("PickupArea");
		area.BodyEntered += OnPickup;
		
		LoadAuraTextures();
		
		SetupAura();
	}

	private static void LoadAuraTextures()
	{
		if (UncommonAura == null && ResourceLoader.Exists(UncommonAuraPath))
		{
			UncommonAura = GD.Load<PackedScene>(UncommonAuraPath);
		}
		if (RareAura == null && ResourceLoader.Exists(RareAuraPath))
		{
			RareAura = GD.Load<PackedScene>(RareAuraPath);
		}
		if (EpicAura == null && ResourceLoader.Exists(EpicAuraPath))
		{
			EpicAura = GD.Load<PackedScene>(EpicAuraPath);
		}
		if (LegendaryAura == null && ResourceLoader.Exists(LegendaryAuraPath))
		{
			LegendaryAura = GD.Load<PackedScene>(LegendaryAuraPath);
		}
	}

	public static void SetAuraScenes(PackedScene uncommon, PackedScene rare, PackedScene epic, PackedScene legendary)
	{
		UncommonAura = uncommon;
		RareAura = rare;
		EpicAura = epic;
		LegendaryAura = legendary;
	}

	private void SetupAura()
	{
		// Remove existing aura if it exists
		var existingAura = GetNodeOrNull<Node2D>("Aura");
		if (existingAura != null)
		{
			existingAura.QueueFree();
		}

		// Get the appropriate aura scene based on rarity
		PackedScene auraScene = Rarity switch
		{
			UpgradeRarity.Uncommon => UncommonAura,
			UpgradeRarity.Rare => RareAura,
			UpgradeRarity.Epic => EpicAura,
			UpgradeRarity.Legendary => LegendaryAura,
			_ => UncommonAura
		};

		if (auraScene != null)
		{
			// Instantiate the aura scene
			var auraInstance = auraScene.Instantiate<Node2D>();
			if (auraInstance != null)
			{
				auraInstance.Name = "Aura";
				AddChild(auraInstance);
				// Move aura to back (lower z-index) - ensures it renders behind other sprites
				MoveChild(auraInstance, 0);
				// Set z-index to be behind everything
				auraInstance.ZIndex = -1;
			}
		}
	}

	public void SetRarity(UpgradeRarity rarity)
	{
		Rarity = rarity;
		SetupAura();
	}

	private void OnPickup(Node body)
	{
		if (body is Player player)
		{
			// Play collect sound
			if (CollectSound != null)
			{
				CollectSound.Play();
			}

			player.ApplyUpgrade(this); // applies stats
			GD.Print($"{Rarity} {ItemName} collected!");
			QueueFree(); // removes the upgrade scene from the map
		}
	}


	// Get the rarity multiplier
	private float GetRarityMultiplier()
	{
		return Rarity switch
		{
			UpgradeRarity.Uncommon => 0.05f,    // 5%
			UpgradeRarity.Rare => 0.10f,      // 10%
			UpgradeRarity.Epic => 0.20f,      // 20%
			UpgradeRarity.Legendary => 0.35f, // 35%
			_ => 0.05f
		};
	}

	// Store the actual stat increases for removal
	private float _appliedHpIncrease = 0f;
	private float _appliedDmgIncrease = 0f;
	private float _appliedAtkSpdIncrease = 0f;
	private float _appliedDefIncrease = 0f;
	private float _appliedSpdIncrease = 0f;

	public override void Apply(Player player)
	{
		if (player == null)
			return;

		float rarityMultiplier = GetRarityMultiplier();


		_appliedHpIncrease = HpPercent > 0 ? player.GetBaseMaxHP() * rarityMultiplier * HpPercent : 0f;
		_appliedDmgIncrease = DmgPercent > 0 ? player.GetBaseDMG() * rarityMultiplier * DmgPercent : 0f;
		_appliedAtkSpdIncrease = AtkSpdPercent > 0 ? player.GetBaseATKSPD() * rarityMultiplier * AtkSpdPercent : 0f;
		_appliedDefIncrease = DefPercent > 0 ? rarityMultiplier * DefPercent : 0f; // DEF is already a percentage
		_appliedSpdIncrease = SpdPercent > 0 ? player.GetBaseSPD() * rarityMultiplier * SpdPercent : 0f;


		player.MaxHP += _appliedHpIncrease;
		player.HP += _appliedHpIncrease;
		player.DMG += _appliedDmgIncrease;
		player.ATKSPD += _appliedAtkSpdIncrease;
		player.DEF = MathF.Min(1f, player.DEF + _appliedDefIncrease);
		player.SPD += _appliedSpdIncrease;

		string rarityName = Rarity.ToString();
		float rarityPercent = rarityMultiplier * 100f;
		
		GD.Print($"Applied {rarityName} upgrade: {ItemName} ({rarityPercent:F0}% increase)");
		if (HpPercent > 0) GD.Print($"  HP: +{_appliedHpIncrease:F1} ({rarityPercent * HpPercent:F1}% of base)");
		if (DmgPercent > 0) GD.Print($"  DMG: +{_appliedDmgIncrease:F1} ({rarityPercent * DmgPercent:F1}% of base)");
		if (AtkSpdPercent > 0) GD.Print($"  ATKSPD: +{_appliedAtkSpdIncrease:F2} ({rarityPercent * AtkSpdPercent:F1}% of base)");
		if (DefPercent > 0) GD.Print($"  DEF: +{_appliedDefIncrease * 100:F1}% ({rarityPercent * DefPercent:F1}% of base)");
		if (SpdPercent > 0) GD.Print($"  SPD: +{_appliedSpdIncrease:F1} ({rarityPercent * SpdPercent:F1}% of base)");
	}

	public override void Remove(Player player)
	{
		if (player == null)
			return;


		player.MaxHP -= _appliedHpIncrease;
		player.DMG -= _appliedDmgIncrease;
		player.ATKSPD -= _appliedAtkSpdIncrease;
		player.DEF = MathF.Max(0f, player.DEF - _appliedDefIncrease);
		player.SPD -= _appliedSpdIncrease;

		if (player.HP < 0)
			player.HP = 0;

		_appliedHpIncrease = 0f;
		_appliedDmgIncrease = 0f;
		_appliedAtkSpdIncrease = 0f;
		_appliedDefIncrease = 0f;
		_appliedSpdIncrease = 0f;
	}
}
