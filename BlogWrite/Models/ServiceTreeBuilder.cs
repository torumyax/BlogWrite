﻿using BlogWrite.Models.Clients;
using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace BlogWrite.Models
{
    // Container class for Treeview.
    public class ServiceTreeBuilder : NodeTree
    {
        public ServiceTreeBuilder() { }

        // Loads service tree.
        public void LoadXmlDoc(XmlDocument doc)
        {
            if (doc == null)
                return;
            if (doc.DocumentElement == null)
                return;

            XmlNodeList accountList;
            accountList = doc.SelectNodes("//Accounts");
            if (accountList == null) 
                return;

            foreach (XmlNode a in accountList)
            {
                // Loop through the top level childs.
                XmlNodeList serviceList = a.ChildNodes;
                if (serviceList == null)
                    continue;

                foreach (XmlNode s in serviceList)
                {
                    if (s.LocalName.Equals("Service"))
                    {
                        var accountName = s.Attributes["Name"].Value;

                        var userName = s.Attributes["UserName"].Value;
                        var userPassword = s.Attributes["UserPassword"].Value;
                        var endpoint = s.Attributes["EndPoint"].Value;
                        string api = (s.Attributes["Api"] != null) ? s.Attributes["Api"].Value : "Unknown"; //
                        string tp = (s.Attributes["Type"] != null) ? s.Attributes["Type"].Value : "Unknown";

                        var selecteds = string.IsNullOrEmpty(s.Attributes["Selected"].Value) ? "" : s.Attributes["Selected"].Value;
                        var expandeds = string.IsNullOrEmpty(s.Attributes["Expanded"].Value) ? "" : s.Attributes["Expanded"].Value;
                        bool isSelecteds = (selecteds == "true") ? true : false;
                        bool isExpandeds = (expandeds == "true") ? true : false;

                        ServiceTypes stp;
                        switch (tp)
                        {
                            case "AtomPub":
                                stp = ServiceTypes.AtomPub;
                                break;
                            case "Feed":
                                stp = ServiceTypes.Feed;
                                break;
                            case "XML-RPC":
                                stp = ServiceTypes.XmlRpc;
                                break;
                            case "AtomAPI":
                                stp = ServiceTypes.AtomApi;
                                break;
                            default:
                                stp = ServiceTypes.Unknown;
                                break;
                        }

                        ApiTypes at;
                        switch (api)
                        {
                            case "AtomPub":
                                at = ApiTypes.atAtomPub;
                                break;
                            case "AtomFeed":
                                at = ApiTypes.atFeed;
                                break;
                            case "RssFeed":
                                at = ApiTypes.atFeed;
                                break;
                            case "Feed":
                                at = ApiTypes.atFeed;
                                break;
                            case "XML-RPC_MovableType":
                                at = ApiTypes.atXMLRPC_MovableType;
                                break;
                            case "XML-RPC_WordPress":
                                at = ApiTypes.atXMLRPC_WordPress;
                                break;
                            case "AtomAPI":
                                at = ApiTypes.atAtomApi;
                                break;
                            default:
                                at = ApiTypes.atUnknown;
                                break;
                        }

                        string viewType = (s.Attributes["ViewType"] != null) ? s.Attributes["ViewType"].Value : "Cards";
                        ViewTypes vt;
                        switch (viewType)
                        {
                            case "Cards":
                                vt = ViewTypes.vtCards;
                                break;
                            case "Magazine":
                                vt = ViewTypes.vtMagazine;
                                break;
                            case "ThreePanes":
                                vt = ViewTypes.vtThreePanes;
                                break;
                            default:
                                vt = ViewTypes.vtCards;
                                break;
                        }

                        if (stp == ServiceTypes.Feed)
                            continue;

                        if ((!string.IsNullOrEmpty(accountName)) && (!string.IsNullOrEmpty(userName)) && (!string.IsNullOrEmpty(userPassword)) && (!string.IsNullOrEmpty(endpoint)))
                        {
                            NodeService account = new NodeService(accountName, userName, userPassword, new Uri(endpoint), at, stp);
                            account.IsSelected = isSelecteds;
                            account.IsExpanded = isExpandeds;
                            account.Parent = this;

                            account.ServiceType = stp;
                            account.Api = at;

                            account.ViewType = vt;


                            XmlNodeList collectionList = s.SelectNodes("Collection");
                            foreach (XmlNode c in collectionList)
                            {
                                var collectionName = c.Attributes["Name"].Value;
                                var selectedc = string.IsNullOrEmpty(c.Attributes["Selected"].Value) ? "" : c.Attributes["Selected"].Value;
                                var expandedc = string.IsNullOrEmpty(c.Attributes["Expanded"].Value) ? "" : c.Attributes["Expanded"].Value;
                                bool isSelectedc = (selectedc == "true") ? true : false;
                                bool isExpandedc = (expandedc == "true") ? true : false;

                                string collectionHref = (c.Attributes["Href"] != null) ? c.Attributes["Href"].Value : "";

                                string collectionId = (c.Attributes["Id"] != null) ? c.Attributes["Id"].Value : "";

                                if ((!string.IsNullOrEmpty(collectionName)) && (!string.IsNullOrEmpty(collectionHref)))
                                {
                                    NodeEntryCollection entries = null;

                                    // TODO:
                                    switch (account.Api)
                                    {
                                        case ApiTypes.atAtomPub:
                                            entries = new NodeAtomPubEntryCollection(collectionName, new Uri(collectionHref), collectionId);
                                            break;
                                        case ApiTypes.atXMLRPC_MovableType:
                                            entries = new NodeXmlRpcEntryCollection(collectionName, new Uri(collectionHref), collectionId);
                                            break;
                                        case ApiTypes.atXMLRPC_WordPress:
                                            entries = new NodeXmlRpcEntryCollection(collectionName, new Uri(collectionHref), collectionId);
                                            break;
                                            //case ApiTypes.atAtomAPI:
                                            //    break;
                                    }

                                    if (entries == null)
                                        continue;

                                    if (entries is NodeAtomPubEntryCollection)
                                    {
                                        if (c.Attributes["CategoriesUri"] != null)
                                        {
                                            var catsUrl = c.Attributes["CategoriesUri"].Value;
                                            if (!string.IsNullOrEmpty(catsUrl))
                                            {
                                                try
                                                {
                                                    Uri catsUri = new(catsUrl);
                                                    (entries as NodeAtomPubEntryCollection).CategoriesUri = catsUri;
                                                }
                                                catch { }
                                            }
                                        }

                                        var catFixed = c.Attributes["IsCategoryFixed"].Value;
                                        if (!string.IsNullOrEmpty(catFixed))
                                        {
                                            if (catFixed == "true")
                                            {
                                                (entries as NodeAtomPubEntryCollection).IsCategoryFixed = true;
                                            }
                                        }

                                        XmlNodeList acceptList = c.SelectNodes("Accept");
                                        foreach (XmlNode act in acceptList)
                                        {
                                            (entries as NodeAtomPubEntryCollection).AcceptTypes.Add(act.InnerText);
                                        }
                                    }

                                    XmlNodeList categoryList = c.SelectNodes("Category");
                                    foreach (XmlNode t in categoryList)
                                    {
                                        var categoryName = t.Attributes["Name"].Value;
                                        var selectedt = string.IsNullOrEmpty(t.Attributes["Selected"].Value) ? "" : t.Attributes["Selected"].Value;
                                        var expandedt = string.IsNullOrEmpty(t.Attributes["Expanded"].Value) ? "" : t.Attributes["Expanded"].Value;
                                        bool isSelectedt = (selectedc == "true") ? true : false;
                                        bool isExpandedt = (expandedc == "true") ? true : false;

                                        if (!string.IsNullOrEmpty(categoryName))
                                        {

                                            NodeCategory category = null;

                                            switch (account.Api)
                                            {
                                                case ApiTypes.atAtomPub:
                                                    category = new NodeAtomPubCategory(categoryName);
                                                    break;
                                                case ApiTypes.atXMLRPC_MovableType:
                                                    category = new NodeXmlRpcMTCategory(categoryName);
                                                    break;
                                                case ApiTypes.atXMLRPC_WordPress:
                                                    category = new NodeXmlRpcWPCategory(categoryName);
                                                    break;
                                                    //case ApiTypes.atAtomAPI:
                                                    //    break;
                                            }

                                            if (category == null)
                                                return;

                                            if (category is NodeAtomPubCategory)
                                            {
                                                (category as NodeAtomPubCategory).Term = categoryName;

                                                var categoryScheme = t.Attributes["Scheme"].Value;
                                                (category as NodeAtomPubCategory).Scheme = categoryScheme;
                                            }


                                            category.IsSelected = isSelectedc;
                                            category.IsExpanded = isExpandedc;
                                            category.Parent = entries;

                                            entries.Children.Add(category);
                                        }
                                    }

                                    entries.IsSelected = isSelectedc;
                                    entries.IsExpanded = isExpandedc;
                                    entries.Parent = account;

                                    account.Children.Add(entries);
                                }
                            }

                            this.Children.Add(account);
                        }
                    }
                    else if (s.LocalName.Equals("Feed"))
                    {
                        NodeFeed feed = LoadXmlChildFeed(s);
                        if (feed == null)
                            continue;

                        feed.Parent = this;

                        if (feed != null)
                            this.Children.Add(feed);

                    }
                    else if (s.LocalName.Equals("Folder"))
                    {
                        var folderName = s.Attributes["Name"].Value;

                        if (!string.IsNullOrEmpty(folderName))
                        {
                            var selecteds = string.IsNullOrEmpty(s.Attributes["Selected"].Value) ? "" : s.Attributes["Selected"].Value;
                            var expandeds = string.IsNullOrEmpty(s.Attributes["Expanded"].Value) ? "" : s.Attributes["Expanded"].Value;
                            bool isSelecteds = (selecteds == "true") ? true : false;
                            bool isExpandeds = (expandeds == "true") ? true : false;

                            NodeFolder folder = new NodeFolder(folderName);
                            folder.IsSelected = isSelecteds;
                            folder.IsExpanded = isExpandeds;
                            folder.Parent = this;

                            int unreadCount = 0;
                            var attr = s.Attributes["UnreadCount"];
                            if (attr != null)
                            {
                                if (!string.IsNullOrEmpty(s.Attributes["UnreadCount"].Value))
                                    unreadCount = int.Parse(s.Attributes["UnreadCount"].Value);
                            }
                            folder.EntryCount = unreadCount;

                            string viewType = (s.Attributes["ViewType"] != null) ? s.Attributes["ViewType"].Value : "Cards";
                            ViewTypes vt;
                            switch (viewType)
                            {
                                case "Cards":
                                    vt = ViewTypes.vtCards;
                                    break;
                                case "Magazine":
                                    vt = ViewTypes.vtMagazine;
                                    break;
                                case "ThreePanes":
                                    vt = ViewTypes.vtThreePanes;
                                    break;
                                default:
                                    vt = ViewTypes.vtCards;
                                    break;
                            }
                            folder.ViewType = vt;



                            XmlNodeList feedList = s.SelectNodes("Feed");
                            foreach (XmlNode f in feedList)
                            {
                                NodeFeed feed = LoadXmlChildFeed(f);
                                if (feed == null)
                                    continue;

                                feed.Parent = folder;

                                if (feed != null)
                                    folder.Children.Add(feed);

                            }

                            this.Children.Add(folder);
                        }
                    }
                }

                break;
            }
        }

        private NodeFeed LoadXmlChildFeed(XmlNode node)
        {
            var feedName = node.Attributes["Name"].Value;

            var selecteds = string.IsNullOrEmpty(node.Attributes["Selected"].Value) ? "" : node.Attributes["Selected"].Value;
            var expandeds = string.IsNullOrEmpty(node.Attributes["Expanded"].Value) ? "" : node.Attributes["Expanded"].Value;
            bool isSelectedf = (selecteds == "true") ? true : false;
            bool isExpandedf = (expandeds == "true") ? true : false;

            var endpoint = node.Attributes["EndPoint"].Value;
            string api = (node.Attributes["Api"] != null) ? node.Attributes["Api"].Value : "Unknown";

            ApiTypes at;
            switch (api)
            {
                case "Feed":
                    at = ApiTypes.atFeed;
                    break;
                default:
                    at = ApiTypes.atUnknown;
                    break;
            }

            string viewType = (node.Attributes["ViewType"] != null) ? node.Attributes["ViewType"].Value : "Cards";
            ViewTypes vt;
            switch (viewType)
            {
                case "Cards":
                    vt = ViewTypes.vtCards;
                    break;
                case "Magazine":
                    vt = ViewTypes.vtMagazine;
                    break;
                case "ThreePanes":
                    vt = ViewTypes.vtThreePanes;
                    break;
                default:
                    vt = ViewTypes.vtCards;
                    break;
            }

            string siteTitle = "";
            var attr = node.Attributes["SiteTitle"];
            if (attr != null)
            {
                siteTitle = string.IsNullOrEmpty(node.Attributes["SiteTitle"].Value) ? "" : node.Attributes["SiteTitle"].Value;
            }

            Uri siteUri = null;
            attr = node.Attributes["SiteUri"];
            if (attr != null)
            {
                var siteLink = string.IsNullOrEmpty(node.Attributes["SiteUri"].Value) ? "" : node.Attributes["SiteUri"].Value;

                if (!string.IsNullOrEmpty(siteLink))
                {
                    try
                    {
                        siteUri = new Uri(siteLink);
                    }
                    catch { }
                }
            }

            int unreadCount = 0;
            attr = node.Attributes["UnreadCount"];
            if (attr != null)
            {
                if (!string.IsNullOrEmpty(node.Attributes["UnreadCount"].Value))
                    unreadCount = int.Parse(node.Attributes["UnreadCount"].Value);
            }

            DateTime lastUpdate = default;
            attr = node.Attributes["LastUpdate"];
            if (attr != null)
            {
                if (!string.IsNullOrEmpty(node.Attributes["LastUpdate"].Value))
                    lastUpdate = DateTime.Parse(node.Attributes["LastUpdate"].Value);
            }


            if (!string.IsNullOrEmpty(endpoint))
            {
                NodeFeed feed = new NodeFeed(feedName, new Uri(endpoint));
                feed.IsSelected = isSelectedf;
                feed.IsExpanded = isExpandedf;
                feed.Parent = this;

                feed.SiteTitle = siteTitle;
                feed.SiteUri = siteUri;

                feed.EntryCount = unreadCount;
                feed.ViewType = vt;
                feed.LastUpdate = lastUpdate;

                feed.Api = at;

                return feed;
            }

            return null;
        }

        // Saves service tree.
        public XmlDocument AsXmlDoc()
        {
            XmlDocument doc = new();
            XmlDeclaration xdec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xdec);

            XmlElement root = doc.CreateElement(string.Empty, "Accounts", string.Empty);
            doc.AppendChild(root);

            foreach (var s in this.Children)
            {
                if (s is NodeService)
                {
                    if (s is NodeFeed)
                    {
                        XmlElement feed = AsXmlFeedElement(doc, (s as NodeFeed));

                        root.AppendChild(feed);
                    }
                    else 
                    {
                        XmlElement service = doc.CreateElement(string.Empty, "Service", string.Empty);

                        XmlAttribute attrs = doc.CreateAttribute("Name");
                        attrs.Value = s.Name;
                        service.SetAttributeNode(attrs);

                        XmlAttribute attrsd = doc.CreateAttribute("Expanded");
                        attrsd.Value = s.IsExpanded ? "true" : "false";
                        service.SetAttributeNode(attrsd);

                        XmlAttribute attrss = doc.CreateAttribute("Selected");
                        attrss.Value = s.IsSelected ? "true" : "false";
                        service.SetAttributeNode(attrss);

                        XmlAttribute attrsn = doc.CreateAttribute("UserName");
                        attrsn.Value = ((NodeService)s).UserName;
                        service.SetAttributeNode(attrsn);

                        XmlAttribute attrsp = doc.CreateAttribute("UserPassword");
                        attrsp.Value = ((NodeService)s).UserPassword;
                        service.SetAttributeNode(attrsp);

                        XmlAttribute attrse = doc.CreateAttribute("EndPoint");
                        attrse.Value = ((NodeService)s).EndPoint.AbsoluteUri;
                        service.SetAttributeNode(attrse);

                        XmlAttribute attrv = doc.CreateAttribute("ViewType");
                        switch (s.ViewType)
                        {
                            case ViewTypes.vtCards:
                                attrv.Value = "Cards";
                                service.SetAttributeNode(attrv);
                                break;
                            case ViewTypes.vtMagazine:
                                attrv.Value = "Magazine";
                                service.SetAttributeNode(attrv);
                                break;
                            case ViewTypes.vtThreePanes:
                                attrv.Value = "ThreePanes";
                                service.SetAttributeNode(attrv);
                                break;
                        }

                        XmlAttribute atstp = doc.CreateAttribute("Type");
                        switch (((NodeService)s).ServiceType)
                        {
                            case ServiceTypes.AtomPub:
                                atstp.Value = "AtomPub";
                                service.SetAttributeNode(atstp);
                                break;
                            case ServiceTypes.Feed:
                                atstp.Value = "Feed";
                                service.SetAttributeNode(atstp);
                                break;
                            case ServiceTypes.XmlRpc:
                                atstp.Value = "XML-RPC";
                                service.SetAttributeNode(atstp);
                                break;
                            case ServiceTypes.AtomApi:
                                atstp.Value = "AtomAPI";
                                service.SetAttributeNode(atstp);
                                break;
                            case ServiceTypes.Unknown:
                                atstp.Value = "Unknown";
                                service.SetAttributeNode(atstp);
                                break;

                        }

                        XmlAttribute atapi = doc.CreateAttribute("Api");
                        switch (((NodeService)s).Api)
                        {
                            case ApiTypes.atAtomPub:
                                atapi.Value = "AtomPub";
                                service.SetAttributeNode(atapi);
                                break;
                            case ApiTypes.atFeed:
                                atapi.Value = "Feed";
                                service.SetAttributeNode(atapi);
                                break;
                            case ApiTypes.atXMLRPC_MovableType:
                                atapi.Value = "XML-RPC_MovableType";
                                service.SetAttributeNode(atapi);
                                break;
                            case ApiTypes.atXMLRPC_WordPress:
                                atapi.Value = "XML-RPC_WordPress";
                                service.SetAttributeNode(atapi);
                                break;
                            case ApiTypes.atAtomApi:
                                atapi.Value = "AtomAPI";
                                service.SetAttributeNode(atapi);
                                break;
                            case ApiTypes.atUnknown:
                                atapi.Value = "Unknown";
                                service.SetAttributeNode(atapi);
                                break;
                        }

                        root.AppendChild(service);


                        foreach (var c in s.Children)
                        {
                            if (!(c is NodeEntryCollection)) continue;

                            XmlElement collection = doc.CreateElement(string.Empty, "Collection", string.Empty);

                            XmlAttribute attrc = doc.CreateAttribute("Name");
                            attrc.Value = c.Name;
                            collection.SetAttributeNode(attrc);

                            XmlAttribute attrcd = doc.CreateAttribute("Expanded");
                            attrcd.Value = c.IsExpanded ? "true" : "false";
                            collection.SetAttributeNode(attrcd);

                            XmlAttribute attrcs = doc.CreateAttribute("Selected");
                            attrcs.Value = c.IsSelected ? "true" : "false";
                            collection.SetAttributeNode(attrcs);

                            XmlAttribute attrch = doc.CreateAttribute("Href");
                            attrch.Value = ((NodeEntryCollection)c).Uri.AbsoluteUri;
                            collection.SetAttributeNode(attrch);

                            XmlAttribute attrid = doc.CreateAttribute("Id");
                            attrid.Value = (c as NodeEntryCollection).Id;
                            collection.SetAttributeNode(attrid);

                            service.AppendChild(collection);

                            if (c is NodeAtomPubEntryCollection)
                            {
                                foreach (var a in (c as NodeAtomPubEntryCollection).AcceptTypes)
                                {
                                    XmlElement acceptType = doc.CreateElement(string.Empty, "Accept", string.Empty);
                                    XmlText xt = doc.CreateTextNode(a);
                                    acceptType.AppendChild(xt);

                                    collection.AppendChild(acceptType);
                                }

                                if ((c as NodeAtomPubEntryCollection).CategoriesUri != null)
                                {
                                    XmlAttribute attrcaturi = doc.CreateAttribute("CategoriesUri");
                                    attrcaturi.Value = (c as NodeAtomPubEntryCollection).CategoriesUri.AbsoluteUri;
                                    collection.SetAttributeNode(attrcaturi);
                                }

                                XmlAttribute attrcatfixed = doc.CreateAttribute("IsCategoryFixed");
                                attrcatfixed.Value = (c as NodeAtomPubEntryCollection).IsCategoryFixed ? "true" : "false";
                                collection.SetAttributeNode(attrcatfixed);
                            }

                            foreach (var t in (c as NodeEntryCollection).Children)
                            {
                                if (!(t is NodeCategory)) continue;

                                XmlElement category = doc.CreateElement(string.Empty, "Category", string.Empty);

                                XmlAttribute attrt = doc.CreateAttribute("Name");
                                attrt.Value = (t).Name;
                                category.SetAttributeNode(attrt);

                                XmlAttribute attrtd = doc.CreateAttribute("Expanded");
                                attrtd.Value = (t).IsExpanded ? "true" : "false";
                                category.SetAttributeNode(attrtd);

                                XmlAttribute attrts = doc.CreateAttribute("Selected");
                                attrts.Value = (t).IsSelected ? "true" : "false";
                                category.SetAttributeNode(attrts);

                                collection.AppendChild(category);

                                if (t is NodeAtomPubCategory)
                                {
                                    //
                                    //XmlAttribute attrtt = doc.CreateAttribute("Term");
                                    //attrtt.Value = ((NodeCategory)t).Term;
                                    //category.SetAttributeNode(attrtt);

                                    //
                                    XmlAttribute attrschme = doc.CreateAttribute("Scheme");
                                    attrschme.Value = (t as NodeAtomPubCategory).Scheme;
                                    category.SetAttributeNode(attrschme);
                                }
                                else if (t is NodeXmlRpcMTCategory)
                                {
                                    //

                                }
                                else if (t is NodeXmlRpcWPCategory)
                                {
                                    //

                                }


                            }


                        }
                    }
                }
                else if (s is NodeFolder)
                {
                    XmlElement folder = doc.CreateElement(string.Empty, "Folder", string.Empty);

                    XmlAttribute attrd = doc.CreateAttribute("Name");
                    attrd.Value = (s).Name;
                    folder.SetAttributeNode(attrd);

                    attrd = doc.CreateAttribute("Expanded");
                    attrd.Value = (s).IsExpanded ? "true" : "false";
                    folder.SetAttributeNode(attrd);

                    attrd = doc.CreateAttribute("Selected");
                    attrd.Value = (s).IsSelected ? "true" : "false";
                    folder.SetAttributeNode(attrd);

                    attrd = doc.CreateAttribute("UnreadCount");
                    attrd.Value = s.EntryCount.ToString();
                    folder.SetAttributeNode(attrd);

                    attrd = doc.CreateAttribute("ViewType");
                    switch (s.ViewType)
                    {
                        case ViewTypes.vtCards:
                            attrd.Value = "Cards";
                            folder.SetAttributeNode(attrd);
                            break;
                        case ViewTypes.vtMagazine:
                            attrd.Value = "Magazine";
                            folder.SetAttributeNode(attrd);
                            break;
                        case ViewTypes.vtThreePanes:
                            attrd.Value = "ThreePanes";
                            folder.SetAttributeNode(attrd);
                            break;
                    }

                    root.AppendChild(folder);

                    foreach (var fd in s.Children)
                    {
                        if (!(fd is NodeFeed)) continue;

                        XmlElement feed = AsXmlFeedElement(doc, (fd as NodeFeed));

                        folder.AppendChild(feed);
                    }
                }

            }

            return doc;
        }

        private XmlElement AsXmlFeedElement(XmlDocument doc, NodeFeed fd)
        {
            XmlElement feed = doc.CreateElement(string.Empty, "Feed", string.Empty);

            XmlAttribute attrf = doc.CreateAttribute("Name");
            attrf.Value = fd.Name;
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("Expanded");
            attrf.Value = fd.IsExpanded ? "true" : "false";
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("Selected");
            attrf.Value = fd.IsSelected ? "true" : "false";
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("EndPoint");
            attrf.Value = fd.EndPoint.AbsoluteUri;
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("Type");
            attrf.Value = "Feed";
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("Api");
            switch (fd.Api)
            {
                case ApiTypes.atFeed:
                    attrf.Value = "Feed";
                    feed.SetAttributeNode(attrf);
                    break;
                case ApiTypes.atUnknown:
                    attrf.Value = "Unknown";
                    feed.SetAttributeNode(attrf);
                    break;
            }

            attrf = doc.CreateAttribute("ViewType");
            switch (fd.ViewType)
            {
                case ViewTypes.vtCards:
                    attrf.Value = "Cards";
                    feed.SetAttributeNode(attrf);
                    break;
                case ViewTypes.vtMagazine:
                    attrf.Value = "Magazine";
                    feed.SetAttributeNode(attrf);
                    break;
                case ViewTypes.vtThreePanes:
                    attrf.Value = "ThreePanes";
                    feed.SetAttributeNode(attrf);
                    break;
            }

            attrf = doc.CreateAttribute("SiteTitle");
            attrf.Value = fd.SiteTitle;
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("SiteUri");
            if (fd.SiteUri != null)
                attrf.Value = fd.SiteUri.AbsoluteUri;
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("UnreadCount");
            attrf.Value = fd.EntryCount.ToString();
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("LastUpdate");
            attrf.Value = fd.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss");
            feed.SetAttributeNode(attrf);

            return feed;
        }
    }
}
