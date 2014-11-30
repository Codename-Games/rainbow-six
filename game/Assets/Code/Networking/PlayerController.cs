using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public Transform FirstPerson;
	public CharacterController CharCont;
	public CharacterMotor CharMotor;
	public Transform ThirdPerson;
	public Player MyPlayer;
	public Vector3 CurPos, lastPos;
	public Quaternion CurRot, lastRot;
	public GameObject deadRag;

	public WalkingState walkingstate = WalkingState.Idle;
	public float WalkSpeed;
	public float RunSpeed;
	public float VelocityMagnitude;

	public Transform thirdAnimation;

	public Material swatMat, isisMat;
	public Renderer thirdMat;

	public bool WasStanding;

	void Start () {
		if(networkView.isMine)
		{
			MyPlayer = NetworkManager.getPlayer(networkView.owner);
			MyPlayer.manager = this;
		}
		if(MyPlayer.Team == 0)
		{
			thirdMat.material = swatMat;
		}
		else
		{
			thirdMat.material = isisMat;
		}
		FirstPerson.gameObject.SetActive(false);
		ThirdPerson.gameObject.SetActive(false);
		DontDestroyOnLoad(gameObject);

		lastRot = CurRot;
		lastPos = CurPos;
	}

	[RPC]
	public void RequestPlayer(string Nameee)
	{
		networkView.RPC("GiveMyPlayer", RPCMode.OthersBuffered, Nameee);
	}
	
	[RPC]
	public void GiveMyPlayer(string n)
	{
		StartCoroutine(GivePlayer(n));
	}
	
	IEnumerator GivePlayer(string nn)
	{
		while(!NetworkManager.HasPlayer(nn))
		{
			yield return new WaitForEndOfFrame();
		}
		MyPlayer = NetworkManager.getPlayer(nn);
		MyPlayer.manager = this;
	}

	void Update () {
	
	}

	void FixedUpdate()
	{
		SpeedController();
		AnimationController();
		VelocityMagnitude = CharCont.velocity.magnitude;

		if(networkView.isMine)
		{
			if(Input.GetKeyDown (KeyCode.K))
				networkView.RPC ("Die", RPCMode.Server);
		}
	}

	public void SpeedController()
	{
		if((Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) && VelocityMagnitude > 0)
		{
			if(Input.GetButton("Sprint"))
			{
				walkingstate = WalkingState.Running;
				CharMotor.movement.maxForwardSpeed = RunSpeed;
				CharMotor.movement.maxSidewaysSpeed = RunSpeed;
				CharMotor.movement.maxBackwardsSpeed = RunSpeed / 2;
			}
			else
			{
				walkingstate = WalkingState.Walking;
				CharMotor.movement.maxForwardSpeed = WalkSpeed;
				CharMotor.movement.maxSidewaysSpeed = WalkSpeed;
				CharMotor.movement.maxBackwardsSpeed = WalkSpeed / 2;
			}
		}
		else
		{
			walkingstate = WalkingState.Idle;
		}
	}

	public void AnimationController()
	{
		if(walkingstate == WalkingState.Running)
		{
			//WalkAnimationHolder.animation["Running"].speed = VelocityMagnitude / RunSpeed * 1.2F;
			//WalkAnimationHolder.animation.CrossFade("Running", 0.2F);
		}
		else if(walkingstate == WalkingState.Walking)
		{
			//WalkAnimationHolder.animation["Walking"].speed = VelocityMagnitude / WalkSpeed * 1.2F;
			//WalkAnimationHolder.animation.CrossFade("Walking", 0.2F);
		}
		else
		{
			//WalkAnimationHolder.animation.CrossFade("Idle", 0.2F);
		}
	}

	[RPC]
	void Server_TakeDamage(float Damage)
	{
		networkView.RPC ("Client_TakeDamage", RPCMode.Server, Damage);
	}
	
	[RPC]
	void Client_TakeDamage(float Damage)
	{
		MyPlayer.Health -= Damage;
		
		if(MyPlayer.Health <= 0)
		{
			networkView.RPC ("Die", RPCMode.All);
			MyPlayer.isAlive = false;
			MyPlayer.Health = 0;
		}
	}
	
	[RPC]
	void Spawn()
	{
		MyPlayer.isAlive = true;
		MyPlayer.Health = 100;
		if(MyPlayer.Team == 0)
		{
			thirdMat.material = swatMat;
		}
		else
		{
			thirdMat.material = isisMat;
		}
		if(networkView.isMine)
		{
			FirstPerson.gameObject.SetActive(true);
			ThirdPerson.gameObject.SetActive (false);
		}
		else
		{
			FirstPerson.gameObject.SetActive(false);
			ThirdPerson.gameObject.SetActive (true);
			thirdAnimation.animation.CrossFade("Take 002");
		}
	}
	
	[RPC]
	void Die()
	{
		MyPlayer.isAlive = false;
		//MyPlayer.Deaths ++;
		FirstPerson.gameObject.SetActive(false);
		ThirdPerson.gameObject.SetActive (false);
		Instantiate(deadRag, ThirdPerson.position, ThirdPerson.rotation);
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		bool moving = false;
		if(stream.isWriting)
		{
			CurPos = FirstPerson.position;
			CurRot = FirstPerson.rotation;
			stream.Serialize(ref CurPos);
			stream.Serialize(ref CurRot);
			if(walkingstate == WalkingState.Walking || walkingstate == WalkingState.Running)
			{
				moving = true;
			}
			else
			{
				moving = false;
			}
			stream.Serialize(ref moving);
			//char Ani = (char)GetComponent<NetworkAnimStates>().CurrentAnim;
			//stream.Serialize(ref Ani);
		}
		else
		{
			stream.Serialize(ref CurPos);
			stream.Serialize(ref CurRot);
			stream.Serialize(ref moving);
			if(moving)
				thirdAnimation.animation.CrossFade("Take 002");
			else
				thirdAnimation.animation.Stop("Take 002");
			ThirdPerson.position = Vector3.Lerp(ThirdPerson.position, CurPos, (Time.time * 0.5f));
			ThirdPerson.rotation = Quaternion.Lerp(ThirdPerson.rotation, CurRot, (Time.time * 0.5f));
			//char Ani = (char)0;
			//stream.Serialize(ref Ani);
			//GetComponent<NetworkAnimStates>().CurrentAnim = (Animations)Ani;
		}
	}
}

public enum WalkingState
{
	Idle,
	Walking,
	Running,
	CrouchingIdle,
	CrouchingWalking
}