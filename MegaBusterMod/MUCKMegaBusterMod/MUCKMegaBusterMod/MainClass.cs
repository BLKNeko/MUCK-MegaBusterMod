using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using R2API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MUCKMegaBusterMod
{
    [BepInPlugin("com.BLKNeko.MegaBusterMod", "MegaBusterMod", "1.0.0.0")]

    public class MainClass : BaseUnityPlugin
    {

        public static MainClass instance;
        public ManualLogSource log;
        public Harmony harmony;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);

            log = Logger;
            harmony = new Harmony("com.BLKNeko.MegaBusterMod");

            Assets.LoadAssetBundle();
            //Assets.LoadSoundbank();
            Assets.PopulateAssets();


            harmony.PatchAll(typeof(AddBuster));
           // harmony.PatchAll(typeof(UseBuster));
            harmony.PatchAll(typeof(UseButtonBuster));
            harmony.PatchAll(typeof(ShootBuster));
            log.LogInfo("Test Buster");

        }

        class Assets
        {

            //-------------------ASSETS

            internal static AssetBundle mainAssetBundle;

            // CHANGE THIS
            private const string assetbundleName = "muckmegabustermod";

            private static string[] assetNames = new string[0];

            public static Mesh MBMesh;

            public static Texture2D MBSprite;

            public static Material MBMat;

            internal static void LoadAssetBundle()
            {
                if (mainAssetBundle == null)
                {
                    using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MUCKMegaBusterMod." + assetbundleName))
                    {
                        mainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                    }
                }

                assetNames = mainAssetBundle.GetAllAssetNames();
            }


            //internal static void LoadSoundbank()
            // {
            //    using (Stream manifestResourceStream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("ZeroModV2.ZeroSB.bnk"))
            //   {
            //         byte[] array = new byte[manifestResourceStream2.Length];
            //         manifestResourceStream2.Read(array, 0, array.Length);
            //         SoundAPI.SoundBanks.Add(array);
            //    }
            // }


            internal static void PopulateAssets()
            {
                if (!mainAssetBundle)
                {
                    Debug.LogError("There is no AssetBundle to load assets from.");
                    return;
                }


                MBSprite = mainAssetBundle.LoadAsset<Texture2D>("MBIcon");

                MBMesh = mainAssetBundle.LoadAsset<Mesh>("MBMesh");

                MBMat = mainAssetBundle.LoadAsset<Material>("MBMat");


            }




            //---------------------- END ASSETS

        }

        class AddBuster
        {
            //------------------------------- ADD ITEM

            public static int BusterID;

            

			// Token: 0x06000010 RID: 16 RVA: 0x00002458 File Offset: 0x00000658
			[HarmonyPatch(typeof(ItemManager), "InitAllItems")]
			[HarmonyPostfix]
			private static void Postfix()
			{
				bool flag = ItemManager.Instance.allItems.Count < 1;
				if (!flag)
				{
					Debug.Log("Adding MegaBuster");
					InventoryItem inventoryItem = ScriptableObject.CreateInstance<InventoryItem>();
					foreach (InventoryItem inventoryItem2 in ItemManager.Instance.allItems.Values)
					{
						bool flag2 = inventoryItem2.name == "Wood Bow";
						if (flag2)
						{
							inventoryItem.Copy(inventoryItem2, 1);
							break;
						}
					}
					inventoryItem.name = "MegaBuster";
					inventoryItem.description = "MegaBuster to shoot";
					inventoryItem.id = ItemManager.Instance.allItems.Count;
					//inventoryItem.mesh = MuckUpgradeBuildings.hammerMesh;
					inventoryItem.mesh = Assets.MBMesh;
					//inventoryItem.material = MuckUpgradeBuildings.hammerMaterial;
					inventoryItem.material = Assets.MBMat;
					//inventoryItem.sprite = Sprite.Create(MuckUpgradeBuildings.hammerSprite, new Rect(0f, 0f, (float)MuckUpgradeBuildings.hammerSprite.width, (float)MuckUpgradeBuildings.hammerSprite.height), new Vector2(0.5f, 0.5f));
					inventoryItem.sprite = Sprite.Create(Assets.MBSprite, new Rect(0f, 0f, (float)Assets.MBSprite.width, (float)Assets.MBSprite.height), new Vector2(0.5f, 0.5f));

					inventoryItem.positionOffset = new Vector3(0, 0, 0);

					inventoryItem.scale = 1;
					
					inventoryItem.rotationOffset = new Vector3(215, 0, 200);

					//inventoryItem.type = InventoryItem.ItemType.Bow;

					inventoryItem.attackDamage = 50;

                    inventoryItem.bowComponent.projectileSpeed = 300f;
                    inventoryItem.bowComponent.nArrows = 1;
                    inventoryItem.bowComponent.angleDelta = 1;

				

					

					


					InventoryItem.CraftRequirement craftRequirement = new InventoryItem.CraftRequirement();
					foreach (InventoryItem inventoryItem3 in ItemManager.Instance.allItems.Values)
					{
						bool flag3 = inventoryItem3.name == "Rock";
						if (flag3)
						{
							craftRequirement.item = inventoryItem3;
							break;
						}
					}
					craftRequirement.amount = 5;
					inventoryItem.requirements = new InventoryItem.CraftRequirement[]
					{
					craftRequirement
					};
					ItemManager.Instance.allItems.Add(inventoryItem.id, inventoryItem);
					Debug.Log("Added MegaBuster");
					BusterID = inventoryItem.id;
				}
			}

			// Token: 0x06000011 RID: 17 RVA: 0x00002640 File Offset: 0x00000840
			[HarmonyPatch(typeof(CraftingUI), "Awake")]
			[HarmonyPostfix]
			private static void CraftingPostfix(CraftingUI __instance)
			{
				InventoryItem[] items = __instance.tabs[1].items;
				InventoryItem[] array = new InventoryItem[items.Length + 1];
				items.CopyTo(array, 0);
				array[items.Length] = ItemManager.Instance.allItems[BusterID];
				__instance.tabs[1].items = array;
			}



            //------------------------------ ADD ITEM END


        }

        
        public class UseBuster
        {
            public static Animator animator;
            public static UseInventory Instance;

            [HarmonyPatch(typeof(UseInventory), "Use")]
            [HarmonyPostfix]
            public static void Postfix(UseInventory __instance)
            {
                Debug.Log("Use MegaBuster");

                InventoryItem inventoryBuster = (InventoryItem)Traverse.Create(__instance).Field("currentItem").GetValue();
                bool flag = inventoryBuster != null && inventoryBuster.name.Contains("MegaBuster");


            }

        }
        


        

        public class UseButtonBuster
        {

            [HarmonyPatch(typeof(UseInventory), "UseButtonUp")]
            [HarmonyPrefix]
            public static bool Postfix(UseInventory __instance)
            {

                InventoryItem inventoryBuster = (InventoryItem)Traverse.Create(__instance).Field("currentItem").GetValue();
                bool flag = inventoryBuster != null && inventoryBuster.name.Contains("MegaBuster");

                if(flag)
                {

                    __instance.animator.Play("Idle");
                    // __instance.eatingEmission.enabled = false;
                    CooldownBar.Instance.HideBar();
                    // __instance.eatSfx.Stop();
                    ClientSend.AnimationUpdate(OnlinePlayer.SharedAnimation.Eat, false);
                    __instance.CancelInvoke();

                    // __instance.chargeSfx.Stop();
                    ClientSend.AnimationUpdate(OnlinePlayer.SharedAnimation.Charge, false);
                    Debug.Log("UseButton MegaBuster");
                    ShootBuster.Postfix(__instance);

                }

                
                return true;

            }

        }

        

        public class ShootBuster
        {


            public static Animator animator;
            public static HitBox hitBox;
            // Token: 0x0400048C RID: 1164
            public static UseInventory Instance;
            // Token: 0x0400048F RID: 1167
            public static TrailRenderer swingTrail;

            // Token: 0x04000490 RID: 1168
            public static RandomSfx swingSfx;


            // Token: 0x04000496 RID: 1174
            public static float eatTime;

            // Token: 0x04000497 RID: 1175
            public static float attackTime;

            // Token: 0x04000498 RID: 1176
            public static float chargeTime;

            // Token: 0x0400049C RID: 1180
            public static InventoryItem currentItem;



            [HarmonyPatch(typeof(UseInventory), "ReleaseWeapon")]
            [HarmonyPrefix]
            public static bool Postfix(UseInventory __instance)
            {

                InventoryItem inventoryBuster = (InventoryItem)Traverse.Create(__instance).Field("currentItem").GetValue();
                bool flag = inventoryBuster != null && inventoryBuster.name.Contains("MegaBuster");

                if (flag)
                {

                    Debug.Log("Shoot MegaBuster");

                    //ShootBuster.animator.Play("Shoot", -1, 0f);

                    float num = 1f;
                    // if (this.IsAnimationPlaying("ChargeHold"))
                    // {
                    //     num = 1f;
                    // }
                    // else
                    // {
                    //    num = __instance.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    //     MonoBehaviour.print("charge: " + num);
                    // }
                    ClientSend.AnimationUpdate(OnlinePlayer.SharedAnimation.Charge, false);
                    __instance.animator.Play("Shoot", -1, 0f);
                    CooldownBar.Instance.HideBar();
                    if (InventoryUI.Instance.arrows.currentItem == null)
                    {
                        return false;
                    }
                    InventoryItem inventoryItem = Hotbar.Instance.currentItem;
                    InventoryItem inventoryItem2 = InventoryUI.Instance.arrows.currentItem;
                    List<Collider> list = new List<Collider>();
                    int num2 = 0;
                    while (num2 < inventoryItem.bowComponent.nArrows && !(InventoryUI.Instance.arrows.currentItem == null))
                    {
                        //inventoryItem2.amount--;
                        if (inventoryItem2.amount <= 0)
                        {
                            InventoryUI.Instance.arrows.currentItem = null;
                        }
                        Vector3 vector = PlayerMovement.Instance.playerCam.position + Vector3.down * 0.5f;
                        Vector3 vector2 = PlayerMovement.Instance.playerCam.forward;
                        float num3 = (float)inventoryItem.bowComponent.angleDelta;
                        vector2 = Quaternion.AngleAxis(-(num3 * (float)(inventoryItem.bowComponent.nArrows - 1)) / 2f + num3 * (float)num2, PlayerMovement.Instance.playerCam.up) * vector2;
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(inventoryItem2.prefab);
                        gameObject.GetComponent<Renderer>().material = inventoryItem2.material;
                        gameObject.transform.position = vector;
                        gameObject.transform.rotation = __instance.transform.rotation;
                        float num4 = (float)Hotbar.Instance.currentItem.attackDamage;
                        float num5 = (float)inventoryItem2.attackDamage;
                        float projectileSpeed = inventoryItem.bowComponent.projectileSpeed;
                        Rigidbody component = gameObject.GetComponent<Rigidbody>();
                        float num6 = 100f * num * projectileSpeed * PowerupInventory.Instance.GetRobinMultiplier(null);
                        component.AddForce(vector2 * num6);
                        Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), PlayerMovement.Instance.GetPlayerCollider(), true);
                        float num7 = num5 * num4;
                        num7 *= num;
                        Arrow component2 = gameObject.GetComponent<Arrow>();
                        component2.damage = (int)(num7 * PowerupInventory.Instance.GetRobinMultiplier(null));
                        component2.fallingWhileShooting = (!PlayerMovement.Instance.grounded && PlayerMovement.Instance.GetVelocity().y < 0f);
                        component2.speedWhileShooting = PlayerMovement.Instance.GetVelocity().magnitude;
                        component2.item = inventoryItem2;
                        ClientSend.ShootArrow(vector, vector2, num6, inventoryItem2.id);
                        list.Add(component2.GetComponent<Collider>());
                        num2++;
                    }
                    foreach (Collider collider in list)
                    {
                        foreach (Collider collider2 in list)
                        {
                            Physics.IgnoreCollision(collider, collider2, true);
                        }
                    }
                    InventoryUI.Instance.arrows.UpdateCell();
                    CameraShaker.Instance.ChargeShake(num);


                    return false;

                }

                return true;


            }

        }


        

    }
}
