allProjects=(
    "StreamingImageSequence_HDRP~"
    "StreamingImageSequence_URP~"
    "StreamingImageSequence~"
)

packageName="com.unity.streaming-image-sequence"

for project in ${allProjects[@]}; do
    cp package.json "${project}/Packages/${packageName}"
    cp CHANGELOG.md "${project}/Packages/${packageName}"    
done

