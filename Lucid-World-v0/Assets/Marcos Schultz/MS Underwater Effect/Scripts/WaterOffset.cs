using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterOffset : MonoBehaviour {

	Vector2 offset1, offset2;
	Material _material;

	void Start(){
		_material = GetComponent<Renderer> ().material;
	}

	void Update () {
		offset1 = new Vector2 (offset1.x + Time.deltaTime*0.01f, offset1.y + Time.deltaTime * 0.02f);
		offset2 = new Vector2 (offset2.x - Time.deltaTime*0.02f, offset2.y - Time.deltaTime * 0.015f);
		_material.SetTextureOffset ("_MainTex", offset1);
		_material.SetTextureOffset ("_DetailAlbedoMap", offset2);
	}
}
