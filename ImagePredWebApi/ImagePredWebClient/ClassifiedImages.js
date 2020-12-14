let imageIndex=0
let images=null

function setImage()
{
    let image=new Image()
    image.src="data: image; base64, "+images[imageIndex].imageBase64
    let p=document.createElement("p")
    let center=document.createElement("center")
    p.innerText="id: "+images[imageIndex].id+", name: "+images[imageIndex].name+
        ",\nclass: "+images[imageIndex].class+", confidence: "+images[imageIndex].confidence+
        ", retrieve count: "+images[imageIndex].retrieveCount
    center.appendChild(image)
    center.appendChild(p)
    let imageDiv=document.getElementById("classifiedImage")
    imageDiv.innerHTML=""
    imageDiv.appendChild(center)
}

async function  onClassSelect()
{
    classes=document.getElementById("classes")
    try 
    {
        let response = await fetch("http://localhost:5000/ClassifiedImages/"+classes.selectedIndex)
        images = await response.json()
        imageIndex=0    
        // console.log(images)
        setImage()
        let bDiv=document.getElementById("buttonsDiv")
        bDiv.innerHTML=""
        if (images.length>1) 
        {
            let center=document.createElement("center")
            let bPrev=document.createElement("button")
            let bNext=document.createElement("button")
            bPrev.textContent="<", bNext.textContent=">"
            bPrev.onclick=onPrevButtonClick
            bNext.onclick=onNextButtonClick
            center.appendChild(bPrev), center.appendChild(bNext)
            bDiv.appendChild(center)
        }
    }
    catch (exc) {window.alert(exc)}
}

function onPrevButtonClick()
{
    if (imageIndex>0) {imageIndex--}
    else {imageIndex=images.length-1}
    setImage()
}

function onNextButtonClick()
{
    if (imageIndex<images.length-1) {imageIndex++}
    else {imageIndex=0}
    setImage()
}

$( async () => 
{
    try 
    {
        let response = await fetch("http://localhost:5000/ClassifiedImages/stats")
        let classes = await response.json()
        for (let i=0; i<classes.length; i++)
        {
            option=new Option()
            option.value=i
            let ending=""
            if (classes[i]!=1) {ending="s"}
            option.text=i+": "+classes[i]+" image"+ending
            if (classes[i]==0) { option.disabled=true }
            document.getElementById("classes").appendChild(option)
        }
    }
    catch (exc) { window.alert(exc) }
    document.getElementById("classes").selectedIndex=-1
})

