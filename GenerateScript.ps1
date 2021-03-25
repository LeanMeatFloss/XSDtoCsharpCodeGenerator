Add-Type -AssemblyName System.Xml.Linq 
Add-Type -AssemblyName System.Linq
[System.Xml.Linq.XElement] $XSDRoot = [System.Xml.Linq.XElement]::Load("AUTOSAR_4-3-0.xsd")
$SolutionInfos = @{}
[System.Linq.Enumerable]::ToArray(($XSDRoot.Elements()))[0]
