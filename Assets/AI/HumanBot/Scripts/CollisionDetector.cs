using UnityEngine;

public class CollisionDetector : MonoBehaviour{

    public HumanBotAgent agent;

    public void Init(HumanBotAgent agent){
        this.agent = agent;
    }

    void OnCollisionEnter(){
        agent.Kill();
	}

}