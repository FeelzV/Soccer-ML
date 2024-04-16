
# Installation

### Référence toolkit site web: https://unity.com/products/machine-learning-agents
### Référence Installation site web: https://unity-technologies.github.io/ml-agents/Installation/
### Notre github https://github.com/FeelzV/Soccer-ML

Assurez vous d'être sur la racine du projet avant de lancer les commandes suivantes.

### installation python directe:
```bash
pip3 install torch -f https://download.pytorch.org/whl/torch_stable.html
pip3 install -e ./ml-agents-envs
pip3 install -e ./ml-agents
```

### installation conda:

```bash
conda create -n env python=3.10.12
conda activate env
pip3 install torch -f https://download.pytorch.org/whl/torch_stable.html
pip3 install -e ./ml-agents-envs
pip3 install -e ./ml-agents
```

Assurez-vous d'ouvrir le projet dans Unity avant de lancer des entraînements. Unity devrait pouvoir trouver la version nécessaire.
Le chemin vers le projet unity : \Soccer-ML\Project
# Lancer un entraînement


Voici le format pour lancer un entraînement:
```bash
mlagents-learn <<path_to_config_file>> --run-id=<<run_id>> --env=<<path_to_unity_env>> --time-scale=<<time_scale>> --num-envs=<<num_envs>>
```

Vous pouvez omettre les arguments --env, time-scale et --num-envs pour lancer un entraînement directement dans la fenêtre Unity.

Il y a aussi un fichier commands.txt avec des commandes pour vous inspirer.

Exemple de format pour lancer des entraînements parallèles:
```bash
mlagents-learn config/poca/SoccerTwosHigherLayers.yaml --run-id=pocaNewKickReward --num-envs=10 --env=./Project/Builds/new_kick/UnityEnvironment.exe --time-scale=10
```

