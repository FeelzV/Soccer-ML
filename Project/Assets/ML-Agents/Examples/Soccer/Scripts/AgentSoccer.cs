using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System.Collections.Generic;


public enum Team
{
    Blue = 0,
    Purple = 1
}

public class AgentSoccer : Agent
{
    // Note that that the detectable tags are different for the blue and purple teams. The order is
    // * ball
    // * own goal
    // * opposing goal
    // * wall
    // * own teammate
    // * opposing player

    public enum Position
    {
        Striker,
        Goalie,
        Generic
    }
    public string purpleGoalTag; //will be used to check if collided with purple goal
    public string blueGoalTag;
    public int test;
    [HideInInspector]
    public Team team;
    float m_KickPower;
    // The coefficient for the reward for colliding with a ball. Set using curriculum.
    float m_BallTouch;
    public Position position;

    const float k_Power = 2000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;


    public GameObject m_blueGoal;
    public GameObject m_purpleGoal;
    [HideInInspector]

    public List<GameObject> m_agents;
    public List<GameObject> m_blueTeam;
    public List<GameObject> m_purpleTeam;


    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        SoccerEnvController envController = GetComponentInParent<SoccerEnvController>();
        if (envController != null)
        {
            m_Existential = 1f / envController.MaxEnvironmentSteps;
        }
        else
        {
            m_Existential = 1f / MaxStep;
        }

        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (m_BehaviorParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
            initialPos = new Vector3(transform.position.x - 5f, .5f, transform.position.z);
            rotSign = 1f;
        }
        else
        {
            team = Team.Purple;
            initialPos = new Vector3(transform.position.x + 5f, .5f, transform.position.z);
            rotSign = -1f;
        }
        if (position == Position.Goalie)
        {
            m_LateralSpeed = 1.0f;
            m_ForwardSpeed = 1.0f;
        }
        else if (position == Position.Striker)
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.3f;
        }
        else
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.0f;
        }
        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        m_blueGoal = GameObject.FindWithTag(blueGoalTag);
        m_purpleGoal = GameObject.FindWithTag(purpleGoalTag);
        m_agents = new List<GameObject>();
        foreach ( var item in envController.AgentsList)
        {
            m_agents.Add(item.Agent.gameObject);
        }
        m_blueTeam = new List<GameObject>();
        m_purpleTeam = new List<GameObject>();
        foreach (var item in m_agents)
        {
            var agentTeam = item.GetComponent<AgentSoccer>().team;
            if (agentTeam == Team.Blue)
            {
                m_blueTeam.Add(item);
            }           
            else
            {
                m_purpleTeam.Add(item);
            }
        }

}

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        m_KickPower = 0f;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * m_ForwardSpeed;
                m_KickPower = 1f;
                break;
            case 2:
                dirToGo = transform.forward * -m_ForwardSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * m_LateralSpeed;
                break;
            case 2:
                dirToGo = transform.right * -m_LateralSpeed;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)

    {

        if (position == Position.Goalie)
        {
            // Existential bonus for Goalies.
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            // Existential penalty for Strikers
            AddReward(-m_Existential);
        }
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;
        }
    }
    /// <summary>
    /// Used to provide a "kick" to the ball.
    /// </summary>
    void OnCollisionEnter(Collision c)
    {
        var force = k_Power * m_KickPower;
        if (position == Position.Goalie)
        {
            force = k_Power;
        }
        if (c.gameObject.CompareTag("ball"))
        {
            var dir = c.contacts[0].point - transform.position;
            dir = dir.normalized;
            if (team == Team.Blue)
            {
                AddRewardKick(dir,m_purpleTeam);
            }
            else
            {
                AddRewardKick(dir, m_purpleTeam);
            }
            
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }
    }

    public override void OnEpisodeBegin()
    {
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
    }

    public void AddRewardKick(Vector3 dir, List<GameObject> agents)
    {
        List<float> reward_list = new List<float>();
        int players_missed = 0;
        foreach (var player in agents)
        {
            var dir_to_player = player.transform.position - transform.position;
            dir_to_player = dir_to_player.normalized;
        float dot_player = Vector3.Dot(dir_to_player, dir);
            if (dot_player > 0.8)
            {
            reward_list.Add(-.05f);
            }
            else
            {
                players_missed++;
            }
        }

        var dir_to_goal = m_purpleGoal.transform.position - transform.position;
        dir_to_goal = dir_to_goal.normalized;
        float dot_goal = Vector3.Dot(dir_to_goal, dir);
        if (dot_goal < 0 && players_missed == agents.Count)
        {
        reward_list.Add(.15f);
        }

        if(dot_goal < 0 )
        {
        reward_list.Add(-.06f);
        }
        else
        {
        reward_list.Add(dot_goal * .2f);
        }

        foreach (var reward in reward_list)
    {
        AddReward(reward * m_BallTouch);
    }
    }

}
