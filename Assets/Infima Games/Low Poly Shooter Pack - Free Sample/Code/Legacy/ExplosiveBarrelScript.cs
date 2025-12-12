using UnityEngine;
using System.Collections;

public class ExplosiveBarrelScript : MonoBehaviour {

	float randomTime;
	bool routineStarted = false;

	//Used to check if the barrel 
	//has been hit and should explode 
	public bool explode = false;

	// Track the spawned destroyed barrel prefab
	private Transform spawnedDestroyedBarrel;

	[Header("Prefabs")]
	//The explosion prefab
	public Transform explosionPrefab;
	//The destroyed barrel prefab
	public Transform destroyedBarrelPrefab;

	[Header("Customizable Options")]
	//Minimum time before the barrel explodes
	public float minTime = 0.05f;
	//Maximum time before the barrel explodes
	public float maxTime = 0.25f;

	[Header("Explosion Options")]
	//How far the explosion will reach
	public float explosionRadius = 12.5f;
	//How powerful the explosion is
	public float explosionForce = 4000.0f;
	//Layer where zombies exist
	public LayerMask zombieLayer;
	
	private void Update () {
		//Generate random time based on min and max time values
		randomTime = Random.Range (minTime, maxTime);

		//If the barrel is hit
		if (explode == true) 
		{
			if (routineStarted == false) 
			{
				//Start the explode coroutine
				StartCoroutine(Explode());
				routineStarted = true;
			} 
		}
	}
	
	private IEnumerator Explode () {
		//Wait for set amount of time
		yield return new WaitForSeconds(randomTime);

		//Spawn the destroyed barrel prefab
		if (destroyedBarrelPrefab != null)
			spawnedDestroyedBarrel = Instantiate (destroyedBarrelPrefab, transform.position, transform.rotation);

		//Explosion force
		Vector3 explosionPos = transform.position;
		Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
		Collider[] zombieColliders = Physics.OverlapSphere(explosionPos, explosionRadius, zombieLayer, QueryTriggerInteraction.UseGlobal);
		
		foreach (Collider hit in zombieColliders)
        {
			if (hit.transform.tag == "Zombie" && hit.TryGetComponent<ZombieController>(out var zombie)) 
			{
				//Apply damage to zombies within radius
				zombie.ApplyDamage(1000);
			}

			if (hit.TryGetComponent<RagdollActivator>(out var ragdoll) && hit.TryGetComponent<ZombieStatsBase>(out var stats) && stats.IsDead())
			{
				//Activate ragdoll on zombies within radius
				ragdoll.ActivateRagdoll();
			}
			
			Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
				rb.AddExplosionForce(explosionForce * 50, explosionPos, explosionRadius);
        }
		
		foreach (Collider hit in colliders) {
			Rigidbody rb = hit.GetComponent<Rigidbody> ();
			
			//Add force to nearby rigidbodies
			if (rb != null)
				rb.AddExplosionForce (explosionForce * 15, explosionPos, explosionRadius);

			//If the barrel explosion hits other barrels with the tag "ExplosiveBarrel"
			if (hit.transform.tag == "ExplosiveBarrel") 
			{
				//Toggle the explode bool on the explosive barrel object
				hit.transform.gameObject.GetComponent<ExplosiveBarrelScript>().explode = true;
			}
				
			//If the explosion hit the tag "Target"
			if (hit.transform.tag == "Target") 
			{
				//Toggle the isHit bool on the target object
				hit.transform.gameObject.GetComponent<TargetScript>().isHit = true;
			}

			//If the explosion hit the tag "GasTank"
			if (hit.GetComponent<Collider>().tag == "GasTank") 
			{
				//If gas tank is within radius, explode it
				hit.gameObject.GetComponent<GasTankScript> ().isHit = true;
				hit.gameObject.GetComponent<GasTankScript> ().explosionTimer = 0.05f;
			}
		}

		//Raycast downwards to check the ground tag
		RaycastHit checkGround;
		if (Physics.Raycast(transform.position, Vector3.down, out checkGround, 100))
		{
			if (explosionPrefab != null)
			{
				//Instantiate explosion prefab at hit position
				Instantiate (explosionPrefab, checkGround.point, 
					Quaternion.FromToRotation (Vector3.forward, checkGround.normal));
			}
		}

		// Deactivate the current barrel object to allow later reuse
		explode = false;
		routineStarted = false;
		gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		// Ensure flags are reset when reactivated
		routineStarted = false;
		explode = false;

		// Destroy the spawned destroyed barrel prefab if it exists
		if (spawnedDestroyedBarrel != null)
		{
			Destroy(spawnedDestroyedBarrel.gameObject);
			spawnedDestroyedBarrel = null;
		}
	}

	void OnDrawGizmos()
	{
		// Draw the explosion radius
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, explosionRadius);
	}
}