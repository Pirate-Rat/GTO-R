using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public Animator anim;
	private GameObject player;

	public float speed = 6;
	public int rotation = 250;

//	Foot Placement
	public Transform Hips;
	public Transform L_Toe;			// Кости ног для FootPlacement'а.
	public Transform R_Toe;			//
	public Transform L_Tip;			//
	public Transform R_Tip;			//

	public float HeelOffset = 5;	// Отступ приаттаченной к поверхности стопы.
	public float SoleOffset = 5;	//

	public RaycastHit LYH, RYH, LZH, RZH;

	private float RStap;			// Вес IK в анимации.
	private float LStap;			//
	private float level;			// Положение нулевой точки (Hips) игрока, относительно поверхности.

//	Gravity Controller
	private static float _speed;
	private static bool isGrounded;	// Персонаж на земле?
	private static bool isJumped;	// Персонаж в прыжке?
	private float vJump;			// Текущее ускорение прыжка.
	private float vGravity;			// Текущее ускорение падения.
	private float maxJump = 0.25f;	// Максимальное ускорение прыжка. Выведена эксперементально.

	public static float x = 0.0f;	// Используется в контроллере камеры
	
	void Start () {
		player	= (GameObject)this.gameObject;
		anim	= GetComponent<Animator>();
		Hips	= anim.GetBoneTransform(HumanBodyBones.Hips);

		_speed = speed;
		isGrounded = true;
		isJumped = false;
	}

	void Update () {
		RStap = anim.GetFloat ("RStap");
		LStap = anim.GetFloat ("LStap");

		PlayerMovement();
		PlayerGravitation();
		SetAnimatorParams();
	}

//	GRAVITATION START
	public void PlayerGravitation () {
		if (LZH.point.y < RZH.point.y) {
			level = Hips.position.y - LZH.point.y;
		} else {
			level = Hips.position.y - RZH.point.y;
		}

		if (level > 1.1 && vJump <= 0) {
			isGrounded = false;
			vGravity = vGravity + 1 * Time.deltaTime;
			player.transform.position -= player.transform.up * vGravity;
		} else if (level > 1.1 && vJump > 0) {
			isGrounded = false;
			vJump = vJump - 1 * Time.deltaTime;
			player.transform.position += player.transform.up * vJump;
		} else if (level < 0.95) {
			player.transform.position += player.transform.up * 3 * Time.deltaTime;
		} else if (isJumped && isGrounded) {
			isJumped = false;
			anim.Play("Jump_Land");
		} else {
			isGrounded = true;
			vJump = 0;
			vGravity = 0;
		}
	}

	public void PlayerMovement() {
		if (isGrounded) {
			if (Input.GetKey(KeyCode.W)) {
				player.transform.position += player.transform.forward * _speed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.S)) {
				player.transform.position -= player.transform.forward * _speed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.A)) {
				player.transform.position -= player.transform.right * _speed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.D)) {
				player.transform.position += player.transform.right * _speed * Time.deltaTime;
			}
			if (Input.GetKey (KeyCode.LeftShift)) { 
				_speed = speed * 2;
			} 
			if (Input.GetKeyUp (KeyCode.LeftShift)) {   
				_speed = speed;
			}
			if(Input.GetKeyDown (KeyCode.Space)) { 
				isJumped = true;
				vJump = maxJump;
				player.transform.position += player.transform.up * vJump;
				anim.Play("Jump_Start");
			}
		} else {
			if (Input.GetKey(KeyCode.W)) {
				player.transform.position += player.transform.forward * _speed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.S)) {
				player.transform.position -= player.transform.forward * _speed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.A)) {
				player.transform.position -= player.transform.right * _speed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.D)) {
				player.transform.position += player.transform.right * _speed * Time.deltaTime;
			}
		}
		Quaternion rotate = Quaternion.Euler (0, x, 0);
		player.transform.rotation = rotate;
	}

	public void SetAnimatorParams () {
		float v;
		float h;
		
		v = Input.GetAxis ("Vertical");
		h = Input.GetAxis ("Horizontal");
		
		anim.SetFloat("vertical", v);
		anim.SetFloat("horizontal", h);
		anim.SetBool ("jump", isJumped);
	}

	public void LateUpdate () {
	}
	
	public void OnAnimatorIK() {
		anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot,1f-LStap);
		anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot,1f-LStap);
		anim.SetIKPositionWeight(AvatarIKGoal.RightFoot,1f-RStap);
		anim.SetIKRotationWeight(AvatarIKGoal.RightFoot,1f-RStap);
		
		Ray L_Y = new Ray(new Vector3(anim.GetIKPosition(AvatarIKGoal.LeftFoot).x,Hips.position.y,anim.GetIKPosition(AvatarIKGoal.LeftFoot).z),new Vector3(0,-1,0));
		Ray R_Y = new Ray(new Vector3(anim.GetIKPosition(AvatarIKGoal.RightFoot).x,Hips.position.y,anim.GetIKPosition(AvatarIKGoal.RightFoot).z),new Vector3(0,-1,0));
		if (Physics.Raycast(L_Y, out LYH)) {
			Vector3 Y = new Vector3(LYH.point.x,LYH.point.y+HeelOffset,LYH.point.z);
			anim.SetIKPosition(AvatarIKGoal.LeftFoot,Y);
			Debug.DrawLine(Hips.position,L_Y.origin,Color.cyan); Debug.DrawLine(L_Y.origin,LYH.point,Color.green);
			Vector3 L_T = new Vector3(L_Toe.position.x,anim.GetIKPosition(AvatarIKGoal.LeftFoot).y,L_Toe.position.z);
			Vector3 L_Sole = new Vector3(L_T.x,L_T.y+HeelOffset*2f,L_T.z); Ray L_Z = new Ray(L_Sole,new Vector3(0,-1,0));
			if (Physics.Raycast(L_Z, out LZH) && LStap == 1) {
				L_Tip.position = anim.GetIKPosition(AvatarIKGoal.LeftFoot); L_Tip.rotation = anim.GetIKRotation(AvatarIKGoal.LeftFoot);
				Vector3 SoleTarget = new Vector3(LZH.point.x,LZH.point.y+SoleOffset,LZH.point.z);
				L_Tip.rotation = Quaternion.FromToRotation(L_Tip.up,LYH.normal); L_Tip.LookAt(SoleTarget);
				anim.SetIKRotation(AvatarIKGoal.LeftFoot,Quaternion.Lerp(anim.GetIKRotation(AvatarIKGoal.LeftFoot),L_Tip.rotation,Time.time));
				Debug.DrawLine(anim.GetIKPosition(AvatarIKGoal.LeftFoot),L_Sole,Color.cyan); Debug.DrawLine(L_Sole,LZH.point,Color.green);
			}
		}
		//
		if (Physics.Raycast(R_Y, out RYH)) {
			Vector3 Y = new Vector3(RYH.point.x,RYH.point.y+HeelOffset,RYH.point.z);
			anim.SetIKPosition(AvatarIKGoal.RightFoot,Y);
			Debug.DrawLine(Hips.position,R_Y.origin,Color.cyan); Debug.DrawLine(R_Y.origin,RYH.point,Color.green);
			Vector3 R_T = new Vector3(R_Toe.position.x,anim.GetIKPosition(AvatarIKGoal.RightFoot).y,R_Toe.position.z);
			Vector3 R_Sole = new Vector3(R_T.x,R_T.y+HeelOffset*2f,R_T.z); Ray R_Z = new Ray(R_Sole,new Vector3(0,-1,0));
			if (Physics.Raycast(R_Z, out RZH) && LStap == 1) {
				R_Tip.position = anim.GetIKPosition(AvatarIKGoal.RightFoot); R_Tip.rotation = anim.GetIKRotation(AvatarIKGoal.RightFoot);
				Vector3 SoleTarget = new Vector3(RZH.point.x,RZH.point.y+SoleOffset,RZH.point.z);
				R_Tip.rotation = Quaternion.FromToRotation(R_Tip.up,RYH.normal); R_Tip.LookAt(SoleTarget);
				anim.SetIKRotation(AvatarIKGoal.RightFoot,Quaternion.Slerp(anim.GetIKRotation(AvatarIKGoal.RightFoot),R_Tip.rotation,Time.time));
				Debug.DrawLine(anim.GetIKPosition(AvatarIKGoal.RightFoot),R_Sole,Color.cyan); Debug.DrawLine(R_Sole,RZH.point,Color.green);
			}
		}
	}
}
