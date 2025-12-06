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
	[Export(PropertyHint.Range, "0,1,0.01")] public float HpPercent = 0f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float DmgPercent = 0f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float AtkSpdPercent = 0f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float DefPercent = 0f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float SpdPercent = 0f;
	

	public UpgradeRarity Rarity { get; set; }

	public static PackedScene UncommonAura;
	public static PackedScene RareAura;
	public static PackedScene EpicAura;
	public static PackedScene LegendaryAura;

	private static readonly string UncommonAuraPath = "res://Scenes/Items/UpgradeRarity/UncommonUpgrade.tscn";
	private static readonly string RareAuraPath = "res://Scenes/Items/UpgradeRarity/RareUpgrade.tscn";
	private static readonly string EpicAuraPath = "res://Scenes/Items/UpgradeRarity/EpicUpgrade.tscn";
	private static readonly string LegendaryAuraPath = "res://Scenes/Items/UpgradeRarity/LegendaryUpgrade.tscn";

	[Export] public AudioStreamPlayer2D CollectSound;

	
	private bool collected = false;

	public static UpgradeRarity GetRandomRarity()
	{
		float roll = (float)GD.RandRange(0f, 100f);
		
		if (roll < 55f)
			return UpgradeRarity.Uncommon;
		else if (roll < 85f)
			return UpgradeRarity.Rare;
		else if (roll < 95f)
			return UpgradeRarity.Epic;
		else
			return UpgradeRarity.Legendary;
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
			UncommonAura = GD.Load<PackedScene>(UncommonAuraPath);

		if (RareAura == null && ResourceLoader.Exists(RareAuraPath))
			RareAura = GD.Load<PackedScene>(RareAuraPath);

		if (EpicAura == null && ResourceLoader.Exists(EpicAuraPath))
			EpicAura = GD.Load<PackedScene>(EpicAuraPath);

		if (LegendaryAura == null && ResourceLoader.Exists(LegendaryAuraPath))
			LegendaryAura = GD.Load<PackedScene>(LegendaryAuraPath);
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
			var auraInstance = auraScene.Instantiate<Node2D>();
			if (auraInstance != null)
			{
				auraInstance.Name = "Aura";
				AddChild(auraInstance);
				MoveChild(auraInstance, 0);
				auraInstance.ZIndex = -1;
			}
		}
	}

	public void SetRarity(UpgradeRarity rarity)
	{
		Rarity = rarity;
		SetupAura();
	}
	
	public string GetStatSummary()
	{
		float rarityMult = GetRarityMultiplier();

		string summary = "";

		if (HpPercent > 0)
			summary += $"HP: {(1 + HpPercent * rarityMult) * 100f:0}%\n";

		if (DmgPercent > 0)
			summary += $"Damage: {(1 + DmgPercent * rarityMult) * 100f:0}%\n";

		if (AtkSpdPercent > 0)
			summary += $"Fire Rate: {(1 + AtkSpdPercent * rarityMult) * 100f:0}%\n";

		if (DefPercent > 0)
			summary += $"Defense: {(1 + DefPercent * rarityMult) * 100f:0}%\n";

		if (SpdPercent > 0)
			summary += $"Speed: {(1 + SpdPercent * rarityMult) * 100f:0}%\n";

		return summary.TrimEnd();
	}


	private void OnPickup(Node body)
	{
		if (collected) return;
		collected = true;

		if (body is Player player)
		{
			player.ApplyUpgrade(this);

			GlobalInventory.Instance?.AddUpgrade(this);

			InventoryUI.Instance?.AddUpgrade(this);

			CollectSound.Play();
			GD.Print($"{Rarity} {ItemName} collected!");

			QueueFree();
		}
	}


	private float GetRarityMultiplier()
	{
		return Rarity switch
		{
			UpgradeRarity.Uncommon => 0.05f,
			UpgradeRarity.Rare => 0.10f,
			UpgradeRarity.Epic => 0.20f,
			UpgradeRarity.Legendary => 0.35f,
			_ => 0.05f
		};
	}

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
		_appliedDefIncrease = DefPercent > 0 ? rarityMultiplier * DefPercent : 0f;
		_appliedSpdIncrease = SpdPercent > 0 ? player.GetBaseSPD() * rarityMultiplier * SpdPercent : 0f;

		player.MaxHP += _appliedHpIncrease;
		player.HP += _appliedHpIncrease;
		player.DMG += _appliedDmgIncrease;
		player.ATKSPD += _appliedAtkSpdIncrease;
		player.DEF = MathF.Min(1f, player.DEF + _appliedDefIncrease);
		player.SPD += _appliedSpdIncrease;

		GD.Print($"Applied {Rarity} upgrade: {ItemName}");
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

		// Ensure HP doesn't exceed MaxHP after removal
		if (player.HP > player.MaxHP)
			player.HP = player.MaxHP;
		
		// Ensure HP doesn't go below 0
		if (player.HP < 0)
			player.HP = 0;

		_appliedHpIncrease = 0f;
		_appliedDmgIncrease = 0f;
		_appliedAtkSpdIncrease = 0f;
		_appliedDefIncrease = 0f;
		_appliedSpdIncrease = 0f;
	}
}
