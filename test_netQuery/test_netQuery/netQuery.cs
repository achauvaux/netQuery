using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace netQuery
{
    public class netQuery
    {
        HtmlDocument doc;
        ArrayList allElements;
        ArrayList elements;

        public netQuery(HtmlDocument pdoc)
        {
            doc = pdoc;
            allElements = getAllElementsFromDoc();
        }

        public netQuery(HtmlDocument pdoc, ArrayList pelements)
        {
            doc = pdoc;
            allElements = pelements;
        }

        public void refresh()
        {
            allElements = getAllElementsFromDoc();
        }

        private void clear()
        {
            elements = allElements;
        }

        public HtmlElement getElement(string pcssExp)
        {
            ArrayList element = getElements(pcssExp);
            if (element != null && element.Count == 1)
            {
                return (HtmlElement)element[0];
            }
            else
            {
                return null;
            }
        }

        public ArrayList getElements(string pcssExp)
        {
            try
            {
                string previousToken = "";
                ArrayList elements2 = new ArrayList();
                ArrayList elementsToRemove;
                ArrayList listTokens = getListTokens(pcssExp);

                clear();

                foreach (string token in listTokens)
                {
                    if (isWord(token))
                    {
                        switch (previousToken)
                        {
                            case (""):
                            case (" "):
                            case (">"):
                            case ("+"):
                                elements = getElementsByAttribute(elements, "tag", token);
                                break;
                            case ("#"):
                                elements = getElementsByAttribute(elements, "id", token);
                                break;
                            case ("."):
                                elements = getElementsByAttribute(elements, "classname", token);
                                break;
                            case (":"):
                                int nth;
                                string pseudoClassName = getPseudoClassName(token);
                                switch (pseudoClassName)
                                {
                                    case ("first-child"):
                                        elements = getNthChildElements(elements, 1);
                                        break;
                                    case ("last-child"):
                                        elements = getLastChildElements(elements);
                                        break;
                                    case ("nth-child"):
                                        nth = Convert.ToInt32(getArgumentValue(token));
                                        elements = getNthChildElements(elements, nth);
                                        break;
                                    /*
                                    case ("lt"):
                                        break;
									*/
                                    case ("eq"):
                                        nth = Convert.ToInt32(getArgumentValue(token));
                                        elements = getNthElement(elements, nth + 1);
                                        break;
                                    case "not":
                                        string cssExp = getArgumentValue(token);
                                        netQuery nq = new netQuery(doc, elements);
                                        elementsToRemove = nq.getElements(cssExp);
                                        foreach (HtmlElement element in elementsToRemove)
                                        {
                                            elements.Remove(element);
                                        }
                                        break;
                                    default:
                                        return new ArrayList();
                                }
                                break;
                            case ("["):
                            string value = null;
                                string[] attVal = token.Split(new Char[] { '=' });
                                string attributeName = attVal[0];
                                if (attVal.Count() > 1)
                                    value = cleanValue(attVal[1]);
                                elements = getElementsByAttribute(elements, attributeName, value);
                                break;
                        }
                    }
                    else
                    {
                        switch (token)
                        {
                            case (" "):
                                elements = getAllDescensdantsElements(elements);
                                break;
                            case (">"):
                                elements = getChildrenElements(elements);
                                break;
                            case ("+"):
                                elements = getYoungerBrothersElements(elements, true);
                                break;
                            case ("~"):
                                elements = getYoungerBrothersElements(elements, false);
                                break;
                        }
                    }

                    previousToken = token;
                }

                return elements;
            }
            catch (Exception e)
            {
                return new ArrayList(); ;
            }
        }

        ArrayList getAllElementsFromDoc()
        {
            ArrayList listAllElements = new ArrayList();
            HtmlElementCollection AllElements = doc.All;

            foreach (HtmlElement element in AllElements)
            {
                listAllElements.Add(element);
            }

            return listAllElements;
        }

        private ArrayList getListTokens(string pcssExp)
        {
            string c;
            string token = "";
            string escapeSequence = "";
            ArrayList listTokens = new ArrayList();

            pcssExp = cleanExp(pcssExp);

            for (int i = 0; i < pcssExp.Length; i++)
            {
                c = pcssExp.Substring(i, 1);
                if (escapeSequence == "")
                {
                    if (" #.>+~[]():\"'".IndexOf(c) == -1)
                    {
                        token += c;
                    }
                    else
                    {
                        switch (c)
                        {
                            case "'":
                                escapeSequence = "'";
                                break;
                            case "\"":
                                escapeSequence = "\"";
                                break;
                            case "[":
                                escapeSequence = "]";
                                listTokens.Add(token);
                                listTokens.Add(c);
                                token = "";
                                break;
                            case "(":
                                escapeSequence = ")";
                                token += c;
                                break;
                            case " ":
                            case "#":
                            case ".":
                            case ">":
                            case "+":
                            case "~":
                            case ":":
                                listTokens.Add(token);
                                listTokens.Add(c);
                                token = "";
                                break;
                        }
                    }
                }
                else
                {
                    if (c == escapeSequence)
                    {
                        if (!(c == "\"" || c == "'"))
                        {
                            if (c == ")")
                                token += c;

                            listTokens.Add(token);

                            if (c != ")")
                                listTokens.Add(c);

                            token = "";
                        }
                        escapeSequence = "";
                    }
                    else
                    {
                        token += c;
                    }
                }
            }

            listTokens.Add(token);

            return listTokens;
        }

        private bool isWord(string ptoken)
        {
            if (" #.>+~[]:".IndexOf(ptoken) == -1)
            {
                return true;
            }

            return false;
        }

        private string getPseudoClassName(string ptoken)
        {
            int pos = ptoken.IndexOf("(");
            if (pos > 0)
            {
                return ptoken.Substring(0, pos);
            }
            else
            {
                return ptoken;
            }
        }

        private string getArgumentValue(string ptoken)
        {
            int pos1 = ptoken.IndexOf("(");
            int pos2 = ptoken.IndexOf(")");
            return ptoken.Substring(pos1 + 1, pos2 - pos1 - 1);
        }

        private string getValueNear(string pexp, string pdelim, bool pleft)
        {
            return null;
        }

        private ArrayList getElementsByAttribute(ArrayList pelements, string pattributeName, string pvalue)
        {
            ArrayList elementsByAttribute = new ArrayList();
            ArrayList descendantsByAttribute = new ArrayList();

            if (pattributeName == "id")
            {
                if (doc.GetElementById(pvalue) != null)
                    addOnce(elementsByAttribute, doc.GetElementById(pvalue));
            }
            else
            {
                foreach (HtmlElement element in pelements)
                {
                    if (pattributeName == "tag")
                    {
                        if (element.TagName == pvalue.ToUpper())
                        {
                            addOnce(elementsByAttribute, element);
                        }
                    }
                    else if (pattributeName == "classname")
                    {
                        string s = element.GetAttribute("classname");
                        string[] classNames = element.GetAttribute("classname").Split(new Char[] { ' ' });
                        for (int i = 0; i < classNames.Length; i++)
                        {
                            if (classNames[i] == pvalue)
                            {
                                addOnce(elementsByAttribute, element);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if(element.GetAttribute(pattributeName)!="")
                        {
                            if(pvalue==null || element.GetAttribute(pattributeName).Replace("[]", "") == pvalue)
                                addOnce(elementsByAttribute, element);
                        }
                    }
                }
            }

            return elementsByAttribute;
        }

        private ArrayList getChildrenElements(HtmlElement pelement)
        {
            ArrayList listChildrenElements = new ArrayList();
            HtmlElementCollection childrenElements = pelement.Children;

            foreach (HtmlElement childElement in childrenElements)
            {
                listChildrenElements.Add(childElement);
            }

            return listChildrenElements;
        }

        private ArrayList getChildrenElements(ArrayList pelements)
        {
            ArrayList listChildrenElements = new ArrayList();

            foreach (HtmlElement element in pelements)
            {
                addElements(listChildrenElements, getChildrenElements(element));
            }

            return listChildrenElements;
        }
        
        private ArrayList getAllDescensdantsElements(HtmlElement pelement)
        {
            ArrayList listDescensdantElements = new ArrayList();

            foreach (HtmlElement childElement in getChildrenElements(pelement))
            {
                listDescensdantElements.Add(childElement);
                addElements(listDescensdantElements, getAllDescensdantsElements(childElement));
            }

            return listDescensdantElements;
        }
        
        private ArrayList getAllDescensdantsElements(ArrayList pelements)
        {
            ArrayList listDescensdantElements = new ArrayList();

            foreach (HtmlElement element in pelements)
            {
                addElements(listDescensdantElements, getAllDescensdantsElements(element));
            }

            return listDescensdantElements;
        }

        private ArrayList getBrotherElements(HtmlElement pelement)
        {
            ArrayList listBrotherElements = new ArrayList();

            addElements(listBrotherElements, getChildrenElements(pelement.Parent));

            return listBrotherElements;
        }

        private ArrayList getBrotherElements(ArrayList pelements)
        {
            ArrayList listBrotherElements = new ArrayList();

            foreach (HtmlElement element in pelements)
            {
                addElements(listBrotherElements, getBrotherElements(element));
            }

            return listBrotherElements;
        }

        private ArrayList getYoungerBrothersElements(HtmlElement pelement, bool bfirstOnly) // brothers on the right
        {
            ArrayList listBrotherElements = new ArrayList();
            bool isYounger = false;
                 
            foreach (HtmlElement brotherElement in pelement.Parent.Children)
            {
                if (isYounger)
                {
                    listBrotherElements.Add(brotherElement);
                    if (bfirstOnly)
                        break;
                }

                if (brotherElement == pelement)
                    isYounger = true;
            }

            return listBrotherElements;
        }

        private ArrayList getYoungerBrothersElements(ArrayList pelements, bool bfirst)
        {
            ArrayList listYoungerBrotherElements = new ArrayList();

            foreach (HtmlElement element in pelements)
            {
                addElements(listYoungerBrotherElements, getYoungerBrothersElements(element, bfirst));
            }

            return listYoungerBrotherElements;
        }

        /*
        private HtmlElement getNthElement(ArrayList pelements, int pnth)
        {
            return (HtmlElement)pelements[pnth - 1];
        }
        */
        
        private ArrayList getNthElement(ArrayList pelements, int pnth)
        {
            ArrayList nthElement = new ArrayList();

            nthElement.Add(pelements[pnth - 1]);

            return nthElement;
        }

        private ArrayList getNthChildElements(ArrayList pelements, int pnth)
        {
            ArrayList nthChildElement = new ArrayList();

            foreach (HtmlElement element in pelements)
            {
                if (getPositionInSiblings(element) == pnth)
                    nthChildElement.Add(element);
            }

            return nthChildElement;
        }

        private ArrayList getLastChildElements(ArrayList pelements)
        {
            ArrayList nthChildElement = new ArrayList();

            foreach (HtmlElement element in pelements)
            {
                if (getPositionInSiblings(element) == getBrotherElements(element).Count) // TODO optimiser
                    nthChildElement.Add(element);
            }

            return nthChildElement;
        }

        public int getPositionInSiblings(HtmlElement pelement)
        {
            return getBrotherElements(pelement).IndexOf(pelement) + 1;
        }

        private ArrayList deduplicate(ArrayList pelements)
        {
            ArrayList deduplicatedElements = new ArrayList();

            foreach (HtmlElement element in pelements)
            {
                if (!deduplicatedElements.Contains(element))
                {
                    deduplicatedElements.Add(element);
                }
            }

            return deduplicatedElements;
        }

        private void addOnce(ArrayList pal, HtmlElement pelement)
        {
            if (!pal.Contains(pelement))
            {
                pal.Add(pelement);
            }
        }

        private void addElements(ArrayList pal1, ArrayList pal2)
        {
            foreach(HtmlElement element in pal2)
            {
                pal1.Add(element);
            }
        }

        private string cleanExp(string pcssExp)
        {
            string cleanExp;
            Regex rgx;

            rgx = new Regex(@"(\s)*([>+~=])(\s)*");
            cleanExp = rgx.Replace(pcssExp, "$2");

            rgx = new Regex(@"(\s)+");
            cleanExp = rgx.Replace(cleanExp, " ");

            return cleanExp;
        }

        private string cleanValue(string pvalue)
        {
            string cleanValue;
            Regex rgx;

            rgx = new Regex("(')(.*)(')");
            cleanValue = rgx.Replace(pvalue, "$2");

            rgx = new Regex("(\")(.*)(\")");
            cleanValue = rgx.Replace(cleanValue, "$2");

            return cleanValue;
        }

    }
}

