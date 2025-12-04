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

	// Add an upgrade (only if not already collected)
	public void AddUpgrade(Upgrade upgrade)
	{
		if (upgrade == null || collectedUpgrades.Contains(upgrade))
			return;

		collectedUpgrades.Add(upgrade);
	}

	// Get all collected upgrades
	public List<Upgrade> GetUpgrades()
	{
		return new List<Upgrade>(collectedUpgrades);
	}

	// Check if a specific upgrade was collected
	public bool HasUpgrade(Upgrade upgrade)
	{
		return collectedUpgrades.Contains(upgrade);
	}

	// Clear upgrades (optional, for testing or respawn)
	public void Clear()
	{
		collectedUpgrades.Clear();
	}
}
