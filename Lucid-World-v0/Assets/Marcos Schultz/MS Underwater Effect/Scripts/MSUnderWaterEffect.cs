using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class MSWaterClass{
	[Tooltip("The tag of the object that contains the collider(trigger), responsible for defining the water area")]
	public string waterTag = "Respawn";
	[Tooltip("The color of the water below the surface")]
	public Color waterColor = new Color32 (15, 150, 125, 0);
	[Range(0.01f,0.7f)][Tooltip("How much the vortex effect will distort the screen image.")]
	public float vortexDistortion = 0.45f;
	[Range(0.01f,0.7f)][Tooltip("The size of the distortion that the image will receive under the water because of the 'fisheye' effect.")]
	public float fisheyeDistortion = 0.3f;
	[Range(0.01f,0.3f)][Tooltip("The speed of the distortion that the image will receive under the water")]
	public float distortionSpeed = 0.2f;
	[Range(0.1f, 0.9f)][Tooltip("The intensity of the color of the water below the surface")]
	public float colorIntensity = 0.3f;
	[Range(0.1f,10)][Tooltip("The visibility underwater")]
	public float visibility = 7;
}

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))] 
public class MSUnderWaterEffect : MonoBehaviour{

	[Header("Waters")]
	[Tooltip("Here you must configure all the types of water you have in your game, according to the tag of each object.")]
	public MSWaterClass[] waters;

	[Space(5)][Header("Water Drops")]
	[Tooltip("If this variable is true, the water droplets on the screen will not appear.")]
	public bool disableDropsOnScreen = false;
	[Tooltip("The texture that will give the effect of drops on the screen")]
	public Texture waterDropsTexture;

	[Space(5)][Header("Sounds")]
	[Tooltip("The sound that will be played when the player enters the water")]
	public AudioClip soundToEnter;
	[Tooltip("The sound that will be played when the player exits the water")]
	public AudioClip soundToExit;
	[Tooltip("The sound that will be played while the player is underwater")]
	public AudioClip underWaterSound;

	[Space(5)][Header("Resources")]
	[Tooltip("Shader 'SrBlur' must be associated with this variable")]
	public Shader SrBlur;
	[Tooltip("Shader 'SrEdge' must be associated with this variable")]
	public Shader SrEdge;
	[Tooltip("Shader 'SrFisheye' must be associated with this variable")]
	public Shader SrFisheye;
	[Tooltip("Shader 'SrVortex' must be associated with this variable")]
	public Shader SrVortex;
	[Tooltip("Shader 'SrQuad' must be associated with this variable")]
	public Shader SrQuad;

	bool underWater;
	bool cameOutOfTheWater;
	bool enableQuadDrops;
	float timerDrops;
	GameObject quadDrops;
	Renderer quadDropsRenderer;

	int waterIndex = 0;

	int interactions = 3;
	float strengthX = 0.00f;
	float strengthY = 0.00f;
	float blurSpread = 0.6f;
	float angleVortex = 0;
	float edgesOnly = 0.0f;

	Color edgesOnlyBgColor = Color.white;
	Vector2 centerVortex = new Vector2(0.5f, 0.5f);
	Material materialBlur = null;
	Material edgeDetectMaterial = null;
	Material fisheyeMaterial = null;
	Material materialVortex = null;
	AudioSource audioSourceCamera;
	GameObject audioSourceUnderWater;
	Camera cameraComponent;
	bool error;

	void OnValidate(){
		Color compareColor = new Color (0.0f, 0.0f, 0.0f, 0.0f);
		for (int x = 0; x < waters.Length; x++) {
			if (waters [x].waterColor == compareColor) {
				waters [x].waterColor = new Color (0.05f, 0.5f, 0.5f, 0.0f);
			}
			if (waters [x].vortexDistortion == 0) {
				waters [x].vortexDistortion = 0.45f;
			}
			if (waters [x].fisheyeDistortion == 0) {
				waters [x].fisheyeDistortion = 0.3f; 
			}
			if (waters [x].distortionSpeed == 0) {
				waters [x].distortionSpeed = 0.2f; 
			}
			if (waters [x].colorIntensity == 0) {
				waters [x].colorIntensity = 0.4f; 
			}
			if (waters [x].visibility == 0) {
				waters [x].visibility = 7; 
			}
		}
		//redundant commands \/
		GetComponent<SphereCollider> ().radius = 0.005f;
		GetComponent<SphereCollider> ().isTrigger = false;
	}

	void Awake (){
		error = false;
		//
		materialVortex = new Material(SrVortex);
		materialVortex.hideFlags = HideFlags.HideAndDontSave;
		materialBlur = new Material(SrBlur);
		materialBlur.hideFlags = HideFlags.DontSave;
		//
		cameraComponent = GetComponent<Camera> ();
		if (!cameraComponent) {
			error = true;
			Debug.LogError ("For the code to function properly, it must be associated with an object that has the camera component.");
			this.gameObject.SetActive (false);
			return;
		}
			
		GetComponent<SphereCollider> ().radius = 0.005f;
		GetComponent<SphereCollider> ().isTrigger = false;
		Rigidbody rbTemp = GetComponent<Rigidbody> ();
		rbTemp.isKinematic = true;
		rbTemp.useGravity = false;
		rbTemp.interpolation = RigidbodyInterpolation.Extrapolate;
		rbTemp.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

		if (!SrQuad.isSupported || disableDropsOnScreen) {
			enableQuadDrops = false;
		} else {
			float cameraScaleFactor = 1 + cameraComponent.nearClipPlane * 25;
			float quadScaleFactor = 1 + cameraComponent.nearClipPlane * 20;
			enableQuadDrops = true;
			quadDrops = GameObject.CreatePrimitive (PrimitiveType.Quad);
			Destroy (quadDrops.GetComponent<MeshCollider> ());
			quadDrops.transform.localScale = new Vector3 (0.16f * quadScaleFactor, 0.16f * quadScaleFactor, 1.0f);
			quadDrops.transform.parent = transform;
			quadDrops.transform.localPosition = new Vector3 (0, 0, 0.05f * cameraScaleFactor);
			quadDrops.transform.localEulerAngles = new Vector3 (0, 0, 0);
			quadDropsRenderer = quadDrops.GetComponent<Renderer> ();
			quadDropsRenderer.material.shader = SrQuad;
			quadDropsRenderer.material.SetTexture ("_BumpMap", waterDropsTexture);
			quadDropsRenderer.material.SetFloat ("_BumpAmt", 0);
		}

		if (underWaterSound) {
			audioSourceUnderWater = new GameObject ("UnderWaterSound");
			audioSourceUnderWater.AddComponent (typeof(AudioSource));
			audioSourceUnderWater.GetComponent<AudioSource> ().loop = true;
			audioSourceUnderWater.transform.parent = transform;
			audioSourceUnderWater.transform.localPosition = new Vector3 (0, 0, 0);
			audioSourceUnderWater.GetComponent<AudioSource> ().clip = underWaterSound;
			audioSourceUnderWater.SetActive (false);
		}

		audioSourceCamera = GetComponent<AudioSource> ();
		audioSourceCamera.playOnAwake = false;
		CheckSupport ();
	}

	void CheckSupport(){
		if (!SrBlur.isSupported) { Debug.LogError ("Shader 'SrBlur' not supported"); }
		if (!SrEdge.isSupported) { Debug.LogError ("Shader 'SrEdge' not supported"); }
		if (!SrFisheye.isSupported) { Debug.LogError ("Shader 'SrFisheye' not supported"); }
		if (!SrVortex.isSupported) { Debug.LogError ("Shader 'SrVortex' not supported"); }
		if (!SrQuad.isSupported) { Debug.LogError ("Shader 'SrQuad' not supported"); }
	}
		
	void OnDisable(){
		if (!error) {
			if (underWaterSound) {
				audioSourceUnderWater.SetActive (false);
			}
			if (enableQuadDrops) {
				timerDrops = 0;
				cameOutOfTheWater = false;
				quadDropsRenderer.material.SetFloat ("_BumpAmt", 0);
			}
		}
	}
		
	void Update (){
		if (!error) {
			if (enableQuadDrops) {
				if (cameraComponent.enabled) {
					quadDrops.SetActive (true);
				} else {
					quadDrops.SetActive (false);
				}
				if (cameOutOfTheWater) {
					timerDrops -= Time.deltaTime * 20.0f;
					quadDropsRenderer.material.SetTextureOffset ("_BumpMap", new Vector2 (0, -timerDrops * 0.01f));
					quadDropsRenderer.material.SetFloat ("_BumpAmt", timerDrops);
					if (timerDrops < 0) {
						timerDrops = 0;
						cameOutOfTheWater = false;
						quadDropsRenderer.material.SetFloat ("_BumpAmt", 0);
					}
				}
			}
			//
			if (underWater) {
				interactions = (int) (7 - (waters [waterIndex].visibility * 0.38f));
				blurSpread = 1 - (waters [waterIndex].visibility * 0.1f);
				edgesOnly = waters [waterIndex].colorIntensity;
				edgesOnlyBgColor = waters [waterIndex].waterColor;
				//
				float fixedTime = waters [waterIndex].distortionSpeed * Time.time * 2.0f;
				float sinVortexAngle = Mathf.Sin(fixedTime * 0.75f) * 10.0f;     // (-10 ~ +10)
				float sinVortexPosX = Mathf.Sin(fixedTime) * 1.3f;               // (-1.3  ~  +1.3f)
				float sinVortexPosY = Mathf.Sin(fixedTime * 0.66f) * 0.45f;      // (-0.45f  ~  +0.45f)
				float sinFisheyeX = (1 + Mathf.Sin(fixedTime)) * 0.25f;          // (0  ~  0.5)
				float sinFisheyeY = (1 + Mathf.Sin(fixedTime * 0.618f)) * 0.25f; // (0  ~  0.5)
				//
				angleVortex = Mathf.Lerp(angleVortex, waters [waterIndex].vortexDistortion * sinVortexAngle, Time.deltaTime * 0.5f); //(-7 ~ +7)
				centerVortex = Vector2.Lerp(centerVortex, new Vector2 (0.5f + sinVortexPosX, 0.5f + sinVortexPosY), Time.deltaTime * 0.5f);
				strengthX = Mathf.Lerp(strengthX, 2.0f * sinFisheyeX * waters [waterIndex].fisheyeDistortion, Time.deltaTime * 0.5f); // (0 ~ distortion)
				strengthY = Mathf.Lerp(strengthY, 2.0f * sinFisheyeY * waters [waterIndex].fisheyeDistortion, Time.deltaTime * 0.5f); // (0 ~ distortion)
			}
		}
	}

	void OnRenderImage (RenderTexture source, RenderTexture destination){
		if (underWater && !error) {
			//effect1 - fisheye
			RenderTexture tmp1 = RenderTexture.GetTemporary (source.width / 2, source.height / 2);
			fisheyeMaterial = CheckShaderAndCreateMaterial (SrFisheye, fisheyeMaterial);
			float ar = (source.width) / (source.height);
			fisheyeMaterial.SetVector ("intensity", new Vector4 (strengthX * ar * 0.15625f, strengthY * 0.15625f, strengthX * ar * 0.15625f, strengthY * 0.15625f));
			Graphics.Blit (source, tmp1, fisheyeMaterial);
			//effect2 - edge
			RenderTexture tmp2 = RenderTexture.GetTemporary (tmp1.width, tmp1.height);
			RenderTexture.ReleaseTemporary (tmp1);
			edgeDetectMaterial = CheckShaderAndCreateMaterial (SrEdge, edgeDetectMaterial);
			edgeDetectMaterial.SetFloat ("_BgFade", edgesOnly);
			edgeDetectMaterial.SetFloat ("_SampleDistance", 0);
			edgeDetectMaterial.SetVector ("_BgColor", edgesOnlyBgColor);
			edgeDetectMaterial.SetFloat ("_Threshold", 0);
			Graphics.Blit (tmp1, tmp2, edgeDetectMaterial, 4);
			//effect3 - vortex
			RenderTexture tmp3 = RenderTexture.GetTemporary (tmp2.width, tmp2.height);
			RenderTexture.ReleaseTemporary (tmp2);
			RenderDistortion (materialVortex, tmp2, tmp3, angleVortex, centerVortex, new Vector2 (1, 1));
			//effect4 - blur
			RenderTexture buffer = RenderTexture.GetTemporary (tmp3.width, tmp3.height, 0);
			DownSample4x (tmp3, buffer);
			for (int i = 0; i < interactions; i++) {
				RenderTexture buffer2 = RenderTexture.GetTemporary (tmp3.width, tmp3.height, 0);
				FourTapCone (buffer, buffer2, i);
				RenderTexture.ReleaseTemporary (buffer);
				buffer = buffer2;
			}
			Graphics.Blit (buffer, destination);
			RenderTexture.ReleaseTemporary (buffer);
			RenderTexture.ReleaseTemporary (tmp3);
		} else {
			Graphics.Blit (source, destination);
		}
	}

    void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
    {
        float off = 0.01f + iteration * blurSpread;
        Graphics.BlitMultiTap(source, dest, materialBlur,
            new Vector2(-off, -off),
            new Vector2(-off, off),
            new Vector2(off, off),
            new Vector2(off, -off)
        );
    }

    void DownSample4x(RenderTexture source, RenderTexture dest)
    {
        //float off = 1.0f;
        //Graphics.BlitMultiTap (source, dest, materialBlur,
        //	new Vector2(-off, -off),
        //	new Vector2(-off,  off),
        //	new Vector2( off,  off),
        //	new Vector2( off, -off)
        //);
    }

    void RenderDistortion(Material material, RenderTexture source, RenderTexture destination, float angle, Vector2 center, Vector2 radius){
		bool invertY = source.texelSize.y < 0.0f;
		if (invertY){
			center.y = 1.0f - center.y;
			angle = -angle;
		}
		Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, angle), Vector3.one);
		material.SetMatrix("_RotationMatrix", rotationMatrix);
		material.SetVector("_CenterRadius", new Vector4(center.x, center.y, radius.x, radius.y));
		material.SetFloat("_Angle", angle*Mathf.Deg2Rad);
		Graphics.Blit(source, destination, material);
	}

	Material CheckShaderAndCreateMaterial (Shader s, Material m2Create){
		if (s.isSupported && m2Create && m2Create.shader == s) {
			return m2Create;
		}
		m2Create = new Material (s);
		m2Create.hideFlags = HideFlags.DontSave;
		return m2Create;
	}

	void OnTriggerEnter (Collider colisor){
		if (enabled && !error) {
			for (int x = 0; x < waters.Length; x++) {
				if (!string.IsNullOrEmpty (waters [x].waterTag)) {
					if (colisor.gameObject.CompareTag (waters [x].waterTag)) {
						underWater = true;
						waterIndex = x;
						//quad
						if (enableQuadDrops) {
							cameOutOfTheWater = false;
							quadDropsRenderer.material.SetFloat ("_BumpAmt", 0);
						}
						//sounds
						if (soundToEnter) {
							audioSourceCamera.clip = soundToEnter;
							audioSourceCamera.PlayOneShot (audioSourceCamera.clip);
						}
						if (underWaterSound) {
							audioSourceUnderWater.SetActive (true);
						}
						//reset variables
						angleVortex = strengthX = strengthY = 0.0f;
						centerVortex = new Vector2(0.5f, 0.5f);
						break;
					}
				}
			}
		}
	}

	void OnTriggerExit (Collider colisor){
		if (enabled && !error) {
			for (int x = 0; x < waters.Length; x++) {
				if (!string.IsNullOrEmpty (waters [x].waterTag)) {
					if (colisor.gameObject.CompareTag (waters [x].waterTag)) {
						underWater = false;
						//quads
						if (enableQuadDrops) {
							cameOutOfTheWater = true;
							timerDrops = 40;
						}
						//sounds
						if (soundToExit) {
							audioSourceCamera.clip = soundToExit;
							audioSourceCamera.PlayOneShot (audioSourceCamera.clip);
						}
						if (underWaterSound) {
							audioSourceUnderWater.SetActive (false);
						}
						break;
					}
				}
			}
		}
	}
}