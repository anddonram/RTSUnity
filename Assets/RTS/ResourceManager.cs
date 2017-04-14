using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace RTS{
public static class ResourceManager  {

		public const int ScrollWidth = 15;
		public static float ScrollSpeed = 25;

		public static float RotateSpeed=100;
		public static float RotateAmount=10;

		public const float MinCameraHeight =  30; 
		public const float MaxCameraHeight =  40; 

		public const float MinCameraAngle = 0;
		public const float MaxCameraAngle = 45;

		private static Vector3 invalidPosition = new Vector3(-99999, -99999, -99999);
		public static Vector3 InvalidPosition { get { return invalidPosition; } }

		public static ResourceType[] ResourceTypes = { ResourceType.Money,ResourceType.Water,ResourceType.Population};

		private static Dictionary<Clan,GameObjectList> clans=new Dictionary<Clan, GameObjectList>();
		private static GenericObjectList genericList;

		public static void SetGenericObjectList(GenericObjectList list){
			genericList = list;
		}
		public static GameObject GetCanvas(){
			return genericList.canvas ;
		}
		public static GameObject GetFog(){
			return genericList.fog ;
		}
		public static Texture2D GetCursor(int index){
			return genericList.cursors[ index];
		}
		public static Material	GetAllowedMaterial(){
			return genericList.allowedMaterial;
		}
		public static void SetGameObjectList(GameObjectList objectList) {
			if(!clans.ContainsKey(objectList.clan))
			clans.Add (objectList.clan, objectList);		
		}

		public static GameObject GetBuilding(string name,Clan clan) {
			return clans[clan].GetBuilding(name);
		}
		
		public static GameObject GetUnit(string name,Clan clan) {
			return clans[clan].GetUnit(name);
		}

		public static GameObject GetBuilding(int value,Clan clan) {
			return clans[clan].GetBuilding(value);
		}
		
		public static GameObject GetUnit(int value,Clan clan) {
			return clans[clan].GetUnit(value);
		}
		public static TechTree GetTechTree(Clan clan) {
			return clans[clan].GetTechTree();
		}
		public static int GetUnitListCount(Clan clan) {
			return clans[clan].units.Length;
		}
		public static int GetBuildingListCount(Clan clan) {
			return clans[clan].buildings.Length;
		}
	}	
	public enum ResourceType { Money, Water,Population }
	public enum Clan { Sig, Eph }
	public enum WOState{Nothing,Attacking,Ridding,
		Harvesting,Emptying,Waiting,Building,
		Rocking,Gating,UnderConstruction,Destroyed,OnFire,Entering}
}
