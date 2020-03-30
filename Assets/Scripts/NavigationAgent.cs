using UnityEngine;
using UnityEngine.AI;


public enum Destination //The next destination of the agent.
{
    ClassroomA,
    Cafeteria,
    DormA,
    ClassroomB,
    Gym,
    DormB
}
public enum State // The state of agent
{
    Healthy,
    InfectedWithSymptoms,
    InfectedNonContagious,
    InfectedContagious,
}
public class NavigationAgent : MonoBehaviour
{
    public bool PatientZero; //Agents marked as PatientZero stay in InfectedContagious all the time.
    public State state;

    private GameObject gymTarget;
    private GameObject hospitalTarget;
    private GameObject classTarget;
    private GameObject cafeteriaTarget;

    private ParamController paramController;
    private IAmClassroom[] classList;
    private IAmCafeteria[] cafeteriaList;

    public Destination destination;
    private float spreadRate;
    private float incubationSpreadRate;
    private float gymFrequency;
    private float activityRate;
    private bool goToHospital;


    private NavMeshAgent studentAgent;
    private Vector3 startPoint;// Keep the start point in memory.




    private int cover = 999;//How long is the incubation period. Initilize with a big number to avoid all agent being infected when start.
    private int infecDay = 999;//The date of being infected.

    //Time spend for going to next destination.
    private int standardTime;
    private int count;

    private int day = 0;//The date of today.

    private bool goOutToday;//If goOutToday is false, stay at dormitory today，which has a relationship with ActivityRate.  
    private bool goBack;// Instead of going to gym, go back to dormitory directly.


    Color orange = new Color(255f / 255f, 112f / 255f, 0f / 255f);

    void Start()
    {
        // Find ParamController
        paramController = FindObjectOfType<ParamController>();

        // Obtain params from paramController
        spreadRate = paramController.SpreadRate;
        incubationSpreadRate = paramController.IncubationSpreadRate;
        gymFrequency = paramController.GymFrequency;
        activityRate = paramController.ActivityRate;
        goToHospital = paramController.GoToHospital;
        hospitalTarget = paramController.HospitalTarget;
        gymTarget = paramController.GymTarget;
        classList = paramController.ClassList;
        cafeteriaList = paramController.CafeteriaList;
        standardTime = paramController.StandardTime * 60;//Relate to paramController.standardTime * 60.Assuming that Unity runs in 60fps.

        studentAgent = GetComponent<NavMeshAgent>();
        startPoint = this.gameObject.transform.position;//Find this point when go back to dormitory.

        count = standardTime;
        studentAgent.destination = startPoint;
        if (PatientZero) // Set PatientZero to InfectedContagious
        {
            state = State.InfectedContagious;
        }
    }


    void FixedUpdate()
    {//Notice that it is FixedUpdate instead of Update
        if (cover + infecDay - 2 <= day)//A new day arrives.-2 is the gap between InfectedContagious and InfectedContagious
            state = State.InfectedContagious;

        if ((cover + infecDay) <= day)// It's time to fully sick. Turn to InfectedWithSymptoms
            state = State.InfectedWithSymptoms;


        if (state == State.InfectedWithSymptoms)//if InfectedWithSymptoms, turn red.
            this.gameObject.GetComponent<Renderer>().material.color = Color.red;
        else if (state == State.InfectedNonContagious)//if InfectedNonContagious, turn yellow.
            this.gameObject.GetComponent<Renderer>().material.color = Color.yellow;
        else if (state == State.InfectedContagious)//if InfectedContagious, turn orange.
            this.gameObject.GetComponent<Renderer>().material.color = new Color(255f / 255f, 112f / 255f, 0f / 255f);

        count++;

        if (count > standardTime)//It's about paramcontroller.StandardTime seconds to go into this function.
        {
            //Update params from UI to make sure the params right. It really matters because user may click the Stop button then click Start button. So you have to flush them.
            spreadRate = paramController.SpreadRate;
            incubationSpreadRate = paramController.IncubationSpreadRate;
            gymFrequency = paramController.GymFrequency;
            activityRate = paramController.ActivityRate;
            goToHospital = paramController.GoToHospital;

            count = 0;
            if (destination == Destination.ClassroomA)//Go ClassroomA
            {
                day++;
                int i = Random.Range(0, 100);
                if (i <= activityRate)
                {
                    goOutToday = true;
                }
                else
                {

                    goOutToday = false;
                }
                Debug.Log("In day " + day.ToString());
                paramController.text = day.ToString();
                studentAgent.enabled = true;
                destination = Destination.Cafeteria;
                int index = Random.Range(0, classList.Length); 
                classTarget = classList[index].gameObject; //Select a random classroom.

                if (goOutToday)
                {
                    //Please Make Sure the index of the target is 0.
                    studentAgent.destination = classTarget.transform.GetChild(0).position;
                }
                else
                {
                    studentAgent.destination = startPoint;
                }
            }
            else if (destination == Destination.Cafeteria)//Go Cafeteria
            {
                studentAgent.enabled = true;
                destination = Destination.DormA;
                int index = Random.Range(0, cafeteriaList.Length);
                cafeteriaTarget = cafeteriaList[index].gameObject;
                if (goOutToday)
                {
                    studentAgent.destination = cafeteriaTarget.transform.GetChild(0).position;
                }

            }
            else if (destination == Destination.DormA)// Go back to dormitory.
            {
                studentAgent.enabled = true;
                destination = Destination.ClassroomB;
                if (goOutToday)
                {

                    studentAgent.destination = startPoint;
                }

            }
            else if (destination == Destination.ClassroomB)// Select a random classroom.
            {
                studentAgent.enabled = true;
                int i = Random.Range(0, 100);
                if (i < gymFrequency) // You have gymFrequency*100 probability to go to gym.
                {
                    destination = Destination.Gym;

                }
                else
                {
                    destination = Destination.DormB;
                }
                int index = Random.Range(0, classList.Length);
                classTarget = classList[index].gameObject; // Select a random classroom.
                if (goOutToday) studentAgent.destination =
                        classTarget.transform.GetChild(0).position;
            }
            else if (destination == Destination.Gym)// Go to gym
            {
                studentAgent.enabled = true;
                goBack = true; // Next time you will go back to dormitory.
                destination = Destination.DormB;
                if (goOutToday)
                {
                    studentAgent.destination = gymTarget.transform.position;
                }

                Debug.Log("gym");
            }
            else if (destination == Destination.DormB)// Go back to dormitory.
            {

                studentAgent.enabled = true;
                if (goBack) // Check if you should go back to dormitory.
                {
                    destination = Destination.ClassroomA;
                    if (goOutToday)
                    {
                        studentAgent.destination = startPoint;
                    }

                    goBack = false;
                }
                else
                {
                    destination = Destination.DormB;
                    if (goOutToday)
                    {
                        studentAgent.destination = startPoint;
                    }
                    goBack = true;
                }
            }


        }
        if (state == State.InfectedWithSymptoms && goToHospital)
        {//If state is InfectedWithSymptoms  and goToHospital is true, then go to hospital.
            studentAgent.destination = hospitalTarget.transform.position;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "stu")//If met other student
        {
            int i = Random.Range(0, 1000);
            if (i <= spreadRate * 1000 / 100 && state == State.InfectedWithSymptoms)// With probability (SpreadRate*1000 /100) to be infected.
                collision.gameObject.SendMessage("GotInfected");// Tell him: you are infected!
            if (i <= incubationSpreadRate * 1000 / 100 && state == State.InfectedContagious)//With probability (incubationSpreadRate*1000 /100) to be infected.
                collision.gameObject.SendMessage("GotInfected");// Tell him: you are infected!
        }
    }

    void GotInfected()
    {
        if (state == State.Healthy)
        {
            infecDay = day;
            cover = Random.Range(3, 7);//Got incubation period.
            state = State.InfectedNonContagious;
        }
    }

}
