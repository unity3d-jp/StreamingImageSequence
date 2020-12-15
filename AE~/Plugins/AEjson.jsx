// May 03 2018 Toshiyuki Mori 

//
// String
//
String.prototype.convertToWindowsDirname = function() {
    var str = this.toString();   
    var re = /^\/[A-z]\//;
    if (str.match(re)) {
        str = str.charAt(1) + ":" + str.substring(2,str.length);
    }
    return str;
}

String.prototype.dirname = function() {
    var OS = $.os;  
    var str = this;
    if (OS.search("Windows") != -1) {  
        str = this.convertToWindowsDirname();
    } 

    var index = str.lastIndexOf("/");
    if ( index < 0 ) {
        return str;
    }
    return str.substring(0,index);
}

String.prototype.filename = function() {
    var index = this.lastIndexOf("/");
    if ( index < 0 ) {
        return this;
    }
    return this.substring(index+1,this.length);
}

String.prototype.basename = function(ext) {
    var slashIndex = lastIndexOf(this, "/" );
    var ret = this.substring(slashIndex + 1, this.length);
    if ( ext ) {
        var extIndex = lastIndexOf(ret, ext);
        if ( extIndex > 0 ) {
            ret = ret.substring(0,extIndex);
        }
    }
    return ret;
}

String.prototype.extname = function() {
    var periodIndex = this.lastIndexOf('.');
    if ( periodIndex < 0) {
        return null;
    }
    var ret = this.substring(periodIndex,this.length);
    
    return ret;
}

//---------------------------------------------------------------------------------------------------------------------

//
// JsonExporter
//

var saveFile = $;
var JsonExporter = function()
{
    this.version = 0.0;
    this.indentLevel = 0;
}

JsonExporter.prototype.getfileNameWithIndex  = function(indexOfPictrure,startIndex)
{
   const zeroString  = '00000';
   
   const prefix = this.filename.substring(0,startIndex);
   const numberString  = (zeroString + indexOfPictrure).slice(-5);
   var ret = prefix + numberString + this.extname;
   return ret;
}

JsonExporter.prototype.print = function(value)
{
    var indentstr = "";
    for (var  i = 0; i < this.indentLevel; i++)
    {
        indentstr += "\t";
    }
    saveFile.writeln(indentstr + value);
}

JsonExporter.prototype.quotePrint = function(value)
{
    var indentstr = "";
    for ( var  i = 0; i < this.indentLevel; i++)
    {
        indentstr += "\t";
    }
    saveFile.writeln(indentstr + "\"" + value + "\"");
}

JsonExporter.prototype.jsonNameAndValue = function(name, value, isContinue)
{
    var indentstr = "";
    for ( var  i = 0; i < this.indentLevel; i++)
    {
        indentstr += "\t";
    }    
    saveFile.writeln(indentstr + "\"" + name + "\"" + ":" + value + (isContinue == true ? "," : ""));
}

JsonExporter.prototype.jsonNameAndString = function(name, value, isContinue)
{
    var indentstr = "";
    for ( var  i = 0; i < this.indentLevel; i++)
    {
        indentstr += "\t";
    }    
    saveFile.writeln(indentstr + "\"" + name + "\"" + ":" + "\"" + value + "\"" + (isContinue == true ? "," : ""));
}

JsonExporter.prototype.jsonKakko = function(name)
{
    if ( name )
    {
       this.print( "\"" + name + "\":{");
    }
    else
    {
       this.print("{");
    }
    this.indentLevel++;    
}

JsonExporter.prototype.jsonArray = function(name,array)
{

    this.print( "\"" + name + "\":[");
    this.indentLevel++;    
    for (  j = 0; j < array.length; j++)
    {
            
        if ( array[j].outputJson )
        {
            array[j].outputJson(j != array.length-1);
        }
        else
        {
            this.print("\"" + array[j]  + ( j != array.length-1 ? "\"," : "\"") );        
        }
    }
    this.indentLevel--;    
    this.print( "]");
}


JsonExporter.prototype.jsonKokka = function(isContinue)
{
    this.indentLevel--;    
    this.print("}" + (isContinue? ",":""));
}

//
// AeLayer
//
var Track = function(item, allFootages,exporter)
{
    this.layer = item;
    this.footages = allFootages;
    this.jsonExporter = exporter ;
}

Track.prototype.findFootage = function(name)
{
    for (  i = 0; i < this.footages.length; i++)
    {
        
         if ( this.footages[i].footageDisplayName == name )
         {
             return this.footages[i].outputName;
         }
    }
    alert("cant find footage.");
}
Track.prototype.outputJson = function(isContinue)
{
    //
    if ( this.layer  instanceof AVLayer ) 
    {
        var avlayer = this.layer;

        var transform = avlayer.transform;
        var position = transform.property("Position");
        var x = position.value[0];
        var y = position.value[1];
        var z = 0.0;
        this.jsonExporter.jsonKakko();
        this.jsonExporter.jsonNameAndValue("Position","[" + x + "," + y + "," + z +  "]",true);
        this.jsonExporter.jsonNameAndValue("Start", avlayer.startTime,true);    
        var duration = avlayer.outPoint - avlayer.startTime;
        this.jsonExporter.jsonNameAndValue("Duration",duration,true);    
        var  footageFileName = saveFile.path.convertToWindowsDirname() +  "/" + this.findFootage(avlayer.source.name);
        this.jsonExporter.jsonNameAndString("Footage",footageFileName,false);        
        this.jsonExporter.jsonKokka(isContinue);  
        
    }
 }
//
// Comp
//
var Comp = function(item, allFootages, exporter)
{
    this.jsonExporter = exporter    
    this.item = item;
    this.trackArray = new Array();
    for ( i = 1; i<= item.layers.length; i++ )
    {
        this.trackArray.push( new Track(item.layers[i], allFootages, exporter) );
    }
}


Comp.prototype.outputJSTIMELINE = function()
{
    var isContinue = true;
    
    this.jsonExporter.jsonKakko();
    this.jsonExporter.jsonNameAndValue("Version", this.jsonExporter.version, isContinue);
//    this.jsonExporter.jsonNameAndString("AssetFolder","", isContinue); // Todo.
    this.jsonExporter.jsonArray("Tracks", this.trackArray);
    isContinue = false;
    this.jsonExporter.jsonKokka(isContinue);
    
 }

var Footage = function(item, exporter ) // constructor
{
    this.jsonExporter = exporter;
    this.item = item;
    this.footageDisplayName = item.name;
    this.firstFilePath  = String(item.file);

    this.dirname = this.firstFilePath.dirname();
    this.filename = this.firstFilePath.filename();
    this.extname = this.filename.extname();
    
    var name =     this.footageDisplayName ;
    var nameEnd = name.indexOf("[");
    if (nameEnd > 0)
    {
        name = name.substring(0, nameEnd -1);
    }
    this.outputName = "Footage_" + name + ".jspseq";    
    
//    this.split_array = this.firstFilePath.split("/"); 
    this.filenameArray = new Array();
    
    var idxOpen = this.footageDisplayName.lastIndexOf("[");
    if ( idxOpen < 0 )
    {
        return ; // not sequential pictures.
    }
    
    var idxClose = this.footageDisplayName.lastIndexOf("]");
    if ( idxClose < 0 )
    {
        return ; // not sequential pictures.
    }
    if ( idxOpen > idxClose )
    {
        return ; // not sequential pictures.
    }
    this.numberString = this.footageDisplayName.substring(idxOpen+1,idxClose);
    var indexOfHyphen = this.numberString.indexOf("-");
    if ( indexOfHyphen < 0 )
    {
        return ; // not sequential pictures.
    }
   this.startNumberString = this.numberString.substring(0,indexOfHyphen);
   this.endNumberString =  this.numberString.substring(indexOfHyphen+1, this.numberString.length);  

    this.startNumber = parseInt(this.startNumberString, 10);
    this.endNumber = parseInt(this.endNumberString, 10);    
    
    for ( var i =  this.startNumber; i <= this.endNumber; i++)
    {
        var filename = this.filename;
        this.filenameArray.push(this.dirname + "/" + this.getfileNameWithIndex(i,idxOpen));
    }
}    


Footage.prototype.getfileNameWithIndex  = function(indexOfPictrure,startIndex)
{
   const zeroString  = '00000';
   
   const prefix = this.filename.substring(0,startIndex);
   const numberString  = (zeroString + indexOfPictrure).slice(-5);
   var ret = prefix + numberString + this.extname;
   return ret;
}

Footage.prototype.jsonResolution = function( isContinue)
{
    this.jsonExporter.jsonKakko("Resolution");
    {
        var isContinue2 = true;
        this.jsonExporter.jsonNameAndValue("Width", this.item.width, isContinue2);
        isContinue2 = false;
        this.jsonExporter.jsonNameAndValue("Height", this.item.height, isContinue2);
    }
    this.jsonExporter.jsonKokka(isContinue);
    
}

Footage.prototype.outputJSPSEQ = function()
{
    var isContinue = true;
    this.jsonExporter.jsonKakko();
    this.jsonExporter.jsonNameAndValue("Version", this.jsonExporter.version, isContinue);
    this.jsonResolution(isContinue);
    this.jsonExporter.jsonArray("Pictures", this.filenameArray);
    isContinue = false;
    this.jsonExporter.jsonKokka(isContinue);
    
 }

ItemCollection.prototype.getAllComps = function()
{
   var curItem, i, comps;
  comps = [];
  for (i = 1; i <= this.length; i += 1) {
    curItem = this[i];
    if (curItem && curItem instanceof CompItem) {
      comps.push(curItem);
    }
  }
  return comps;
   
}

ItemCollection.prototype.getAllFootage = function() {
  var curItem, i, footages;
  footages = [];
  for (i = 1; i <= this.length; i += 1) {
    curItem = this[i];
    if (curItem && curItem instanceof FootageItem) {
      footages.push(curItem);
    }
  }
  return footages;
};


main = function()
{
    var allComps =  app.project.items.getAllComps();
    var allFootagees = app.project.items.getAllFootage();
    var footages = new Array();
    var exporter = new JsonExporter();

    var TmpFile = new File("AeConvert.jstimeline");
    saveFile = TmpFile.saveDlg("");
    if ( !saveFile)
    {
       return;
    }
    if (!saveFile.open("w"))
    {
        alert("cant open file", "Error");
        return;
    }
    var folder = saveFile.path;
    
    for ( i = 0; i < allFootagees.length; i++)
    {
        var footage = new Footage( allFootagees[i],exporter);
        footages.push(footage);
    }    

    for ( i = 0; i < allComps.length; i++)
    {
        if ( i != 0 )
        {
            break;
        }

        var comp = new Comp(allComps[i],footages, exporter)
        comp.outputJSTIMELINE();

    }
    saveFile.close();
    
   ///   $.writeln(folder);
    for ( i = 0; i < allFootagees.length; i++)
    {
        
        var footage = footages[i];
        var name =     footage.footageDisplayName ;
        var nameEnd = name.indexOf("[");
        if (nameEnd > 0)
        {
            name = name.substring(0, nameEnd -1);
        }
        var path = folder + "/" + footage.outputName;
        saveFile = new File (path);
         if (!saveFile.open("w"))
        {
            alert("cant open file", "Error");

            break;
        }
        footage.outputJSPSEQ()
        saveFile.close();
    }   
}
main();

