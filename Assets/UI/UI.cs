using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using RTS;

public class UI : MonoBehaviour
{

	public Text selectedName;
	public Text money, water, population,
		maxMoney, maxWater, maxPopulation, username, clan;

	//Info de recursos para mejoras, construcciones, etc.
	public Text resourceMoney, resourceWater, resourceName,
	resourceDescription;
	public GameObject resourcePanel;



	public Slider healthBar;
	public Text health, healthMax;
	public Slider actionBar;
	public Slider staminaBar;

	public Text[] infoContents;
	public Button[] actionButtonList;

	//Panel del menu
	public GameObject menuPanel;
	public GameObject sellButton;
	public GameObject cancelButton;

	public Player player{ get; set; }


	void Start ()
	{
		sellButton.SetActive (false);
		cancelButton.SetActive (false);
		health.gameObject.SetActive (false);
		healthMax.gameObject.SetActive (false);
		healthBar.gameObject.SetActive (false);
		actionBar.gameObject.SetActive (false);
		staminaBar.gameObject.SetActive (false);
	
	}

	public void UpdateHUD (WorldObject wo)
	{
		selectedName.text = wo.woName;
		healthBar.gameObject.SetActive (true);
		health.gameObject.SetActive (true);
		healthMax.gameObject.SetActive (true);

		username.text = wo.getOwner ().username;
		username.color = wo.getOwner ().teamColor;
		clan.text = wo.getOwner ().clan.ToString ();
		clan.color = wo.getOwner ().teamColor;

		if (wo is Unit) {
			staminaBar.gameObject.SetActive (true);
			UpdateStaminaBar (wo);
		}
		UpdateHealth (wo);
		if (wo.IsOwnedBy (player.netId)) {
			UpdateActionPanel (wo);
			UpdateInfoPanel (wo);
			actionBar.gameObject.SetActive (true);
			UpdateActionBar (wo);
		}
	}

	public void ClearHUD ()
	{
		ClearActionPanel ();
		ClearInfoPanel ();
		actionBar.gameObject.SetActive (false);
		staminaBar.gameObject.SetActive (false);
		healthBar.gameObject.SetActive (false);
		health.gameObject.SetActive (false);
		healthMax.gameObject.SetActive (false);
		selectedName.text = string.Empty;
	}

	public void UpdateInfoPanel (WorldObject wo)
	{
		ClearInfoPanel ();
		string[] infos = wo.GetInfo ();
		for ( int i=0;i<infos.Length;i++ ) {
			infoContents [i].gameObject.SetActive (true);
			infoContents [i].text = infos [i];
		}
	}

	public void UpdateActionPanel (WorldObject wo)
	{
		ClearActionPanel ();
		if (wo is Building) {
			Building b = (Building)wo;

			sellButton.SetActive (true);

			if (b.UnderConstruction ())
				return;
			cancelButton.SetActive (true);
		} 
		string[] actions = wo.GetActions ();
		for (int i = 0; i < actions.Length; i++) {
			actionButtonList [i].gameObject.SetActive (true);
			Text t = actionButtonList [i].GetComponentInChildren<Text> ();
			t.text = actions [i];
		}
	}

	public void UpdateResourceValues (ResourceType type)
	{
		switch (type) {
		case ResourceType.Money:
			money.text = player.GetResource (type).ToString ();
			break;
		case ResourceType.Water:
			water.text = player.GetResource (type).ToString ();
			break;
		case ResourceType.Population:
			population .text = player.GetResource (type).ToString ();
			break;
		}
	
	}

	public void UpdateResourceLimitValues (ResourceType type)
	{
	switch (type) {
		case ResourceType.Money:
			maxMoney.text = player.GetResourceLimit (type).ToString ();
			break;
		case ResourceType.Water:
			maxWater.text = player.GetResourceLimit (type).ToString ();
			break;
		case ResourceType.Population:
			maxPopulation.text = player.GetResourceLimit (type).ToString ();
			break;
		}
	}

	public void UpdateActionBar (WorldObject wo)
	{
		float actionPoints = wo.GetActionPoints ();
		if (actionBar.value != actionPoints && wo.IsOwnedBy (player.netId)) {
			actionBar.value = actionPoints;
		}
	}

	public void UpdateStaminaBar (WorldObject wo)
	{
		float stamina = (wo as Unit).GetStamina ();
		if (staminaBar.value != stamina)
			staminaBar.value = stamina;
	}

	public void UpdateHealth (WorldObject wo)
	{
		float hp = wo.hitPoints;
		float hpMax = wo.maxHitPoints;
		if (healthBar.value != hp) {
			health.text = hp.ToString ();
			healthBar.value = hp;
		}
		if (healthBar.maxValue != hpMax) {
			healthMax.text = hpMax.ToString ();
			healthBar.maxValue = hpMax;
		}
	}

	public void UpdateSelectionBox ()
	{
		ClearActionPanel ();
		List<Unit> units = player.SelectionUnits;
		for (int i = 0; i < units.Count&&i<actionButtonList.Length; i++) {	
			actionButtonList [i].gameObject.SetActive (true);
			Text t = actionButtonList [i].GetComponentInChildren<Text> ();
			t.text = units [i].woName;
		}
	}

	public void ClearActionPanel ()
	{
		sellButton.SetActive (false);
		cancelButton.SetActive (false);
		for (int i = 0; i < actionButtonList.Length; i++)
			actionButtonList[i].gameObject.SetActive (false);
	}

	public void ClearInfoPanel ()
	{
		for (int i = 0; i < infoContents.Length; i++)
			infoContents[i].gameObject.SetActive(false);
	}
	public void UpdateResourcePanel(WorldObject wo){
		resourceName.text = wo.woName;
		resourceMoney.text = wo.moneyCost.ToString();
		resourceWater.text = wo.waterCost.ToString();
		resourceDescription.text = string.Empty;
		resourcePanel.SetActive (true);
	}

	public void Enter(int index){

		if (player.SelectedObject && index < player.SelectedObject.GetActions ().Length) {
			string action =player.SelectedObject.GetActions () [index];

			GameObject g=player.GetBuilding (action);
			if (!g) {
				g = player.GetUnit (action);
			}
			if (g) {
				WorldObject wo = g.GetComponent<WorldObject> ();
				UpdateResourcePanel (wo);

			} else {
				Node n =player.GetNode (action);
				if (n) {
					UpdateResourcePanel (n);
				}
			}

		}

	}
	public void Exit(){
		ClearResourcePanel ();
	}
	public void UpdateResourcePanel(Node node){
		resourceName.text = node.nodeName;
		resourceMoney.text = node.moneyCost.ToString();
		resourceWater.text = node.waterCost.ToString();
		resourceDescription.text = node.description;
		resourcePanel.SetActive (true);
	}
	public void ClearResourcePanel(){
		resourcePanel.SetActive (false);
	}


}
