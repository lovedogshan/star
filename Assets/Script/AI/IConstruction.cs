using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static Constants;




public interface IConstruction
{

}




public interface IConBuild : IConstruction
{






    bool BuildCheck(Vector2Int relPosition, out List<BuildCheckMarkItem> checkMarkItems, float angle = 0);




    bool Build(int buildSheetIndex = 0);
}




public interface IConDestruct : IConstruction
{




    bool Destruct();
}




public interface IConWork : IConstruction
{





    ConstructionState Work();
}




/*public interface IConRepair : IConstruction
{



    List<ResourceItem> RepairResources { get; }




    void Repair();
}*/




public interface IConWorker : IConstruction
{



    bool IsInfiniteTime { get; }



    float TimerMax { get; }



    void WorkerEnd(Person workRole);
}




/*public interface IStore : IConstruction
{



    float StoreSizeMax { get; }



    float CurrentSize { get; }



    List<EntityResource> EntityResources { get; set; }



    System.Func<EntityResource, bool> StoreCondition { get; set; }





    bool CheckStore(EntityResource entityResource);






    bool OnStore(EntityResource entityResource);






    bool OnTake(EntityResource entityResource);




    void OnFree();
}




public interface IStoreAccompany
{
    IStore Store { get; }



    Storer.StorerCondition StorerCondition { get; }
}




public interface IConStudy : IConWork
{



    ScienceKind ScienceKind { get; }



    IStudyProgress StudyProgress { get; }

    float StudySpeed { get; }





    void StartStudy(IStudyProgress studyProgress, int nodeId);



    void SuccessStudy();



    void AbortStudy();
}

public interface IStudyProgress
{
    ScienceKind ScienceKind { get; set; }
    float Progress { get; set; }
}*/