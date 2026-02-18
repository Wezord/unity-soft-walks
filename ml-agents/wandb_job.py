import wandb

def main():
    print("Hello World")

if __name__ == "__main__":
    main()

#wandb job create --project "Dynamical" -e "dynamical" --name "mlagents-learn" code "wandb_job.py"

"""

$scriptpath = $MyInvocation.MyCommand.Path
$folderpath = Split-Path $scriptpath -Parent
$projectpath = Split-Path $folderpath -Parent
$runid = $args[0]
$build = $args[1]
$params = $args[2]
if (!$params) {
    $params = ""
}
$params = $params.Replace("`"","")
conda activate env
$command = "mlagents-learn.exe `"$projectpath\mlagents_config.yaml`" --results-dir=`"$projectpath\results`" --env=`"$projectpath\TrainingBuilds\$build\Dynamical`" --num-envs=12 --no-graphics --run-id=`"$runid`" $params"
Write-Output $command

try
{
    Invoke-Expression $command
}
finally
{
    Copy-Item -Path "$projectpath\results\$runid\*Behavior*.onnx" -Destination "$projectpath\Assets\AI\Models\"
}


"""
