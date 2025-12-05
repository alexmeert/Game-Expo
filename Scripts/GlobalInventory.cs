using Godot;
using System.Collections.Generic;

public partial class GlobalInventory : Node
{
	public static GlobalInventory Instance { get; private set; }

	private List<Upgrade> collectedUpgrades = new List<Upgrade>();

	public override void _EnterTree()
	{
		Instance = this;
	}

	public void AddUpgrade(Upgrade upgrade)
	{
		if (upgrade == null || collectedUpgrades.Contains(upgrade))
			return;

		collectedUpgrades.Add(upgrade);
	}

	public List<Upgrade> GetUpgrades()
	{
		return new List<Upgrade>(collectedUpgrades);
	}

	public bool HasUpgrade(Upgrade upgrade)
	{
		return collectedUpgrades.Contains(upgrade);
	}

	public void ClearUpgrades()
	{
		collectedUpgrades.Clear();
	}
}
