using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallPlat : MonoBehaviour
{
	public float fallTime = 0.5f;


	void OnCollisionEnter(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts)
		{
			//Comprueba si la colision del objeto es con el jugador con la referencia del gameobject
			if (collision.gameObject.tag == "Player")
			{
				//Comprueba, con una corrutina si el jugador ha caido
				StartCoroutine(Fall(fallTime));
			}
		}
	}


	IEnumerator Fall(float time)
	{
		yield return new WaitForSeconds(time);
		Destroy(gameObject);
	}
}
