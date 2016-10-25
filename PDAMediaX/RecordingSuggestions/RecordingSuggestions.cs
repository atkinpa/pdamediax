using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using pdaMediaX.Common;
using pdaMediaX.Util.Xml;
using pdaMediaX.Web;

namespace RecordingSuggestions
{
    class RecordingSuggestions : pdaMediaX.pdamxBatchJob 
    {
        static void Main(string[] args)
        {
            new RecordingSuggestions();
        }
        public RecordingSuggestions()
        {
            DateTimeFormatInfo dtFormat = new CultureInfo("en-US", false).DateTimeFormat;
            //Hashtable hSearchResult = null;
            pdamxXMLReader mxXMLVideoXMLDBReader;
            pdamxXMLReader mxXMLExBackupVideoXMLDBReader = null;
            pdamxXMLWriter mxExVidoeXMLDBWriter;
            pdamxBeyondTV mxBeyondTV;
            XPathNodeIterator xpathINode;

            String sRecordingSuggestionFile = "";
            String sVideoXMLDBLibraryFile = "";
            String sBTVNetworkLicenseFile = "";
            String sBTVUserAccessFile = "";
            String sBTVNetworkLicense = "";

            int nNumberOfMoviesRead = 0;
            int nNumberOfRecordingSuggestionsFound = 0;

            String jobInfoXMLTemplate =
                  "\n   <JobInfo>"
                + "\n      <Generated></Generated>"
                + "\n      <Generator></Generator>"
                + "\n       <Machine></Machine>"
                + "\n      <OS></OS>"
                + "\n      <OSVersion></OSVersion>"
                + "\n   </JobInfo>";

            String movieXMLTemplate =
                      "\n   <Movie>"
                    + "\n     <Title></Title>"
                    + "\n     <Description></Description>"
                    + "\n     <Credits></Credits>"
                    + "\n     <MovieYear></MovieYear>"
                    + "\n     <ParentalRating></ParentalRating>"
                    + "\n   </Movie>";

            String summaryXMLTemplate =
                      "\n   <Summary>"
                    + "\n     <MoviesRead></MoviesRead>"
                    + "\n     <RecordingSuggestionsFound></RecordingSuggestionsFound>"
                    + "\n   </Summary>";

        }
    }
}
