﻿using EPiServer.Core;
using EPiServer.SpecializedProperties;
using EPiServer.Web.Mvc;
using EPiServerMvcBootstrap.Models;
using EPiServerMvcBootstrap.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using Castle.DynamicProxy;
using EPiServer.ServiceLocation;

namespace EPiServerMvcBootstrap.Controllers
{
    public abstract class ApplicationPageController<T> : PageController<T> where T : TypedPageData
    {
        private readonly IContentRepository _contentRepository;

        protected IContentRepository ContentRepository
        {
            get { return _contentRepository ?? ServiceLocator.Current.GetInstance<IContentRepository>(); }
        }

        public ApplicationPageController()
        {
        }

        /// <summary>
        /// Gets the current page and casts it to the current page type.
        /// </summary>
        public T CurrentPage
        {
            get
            {
                var currentPage = DataFactory.Instance.CurrentPage;

                if (currentPage == null)
                {
                    var pageRouteHelper = EPiServer.ServiceLocation.ServiceLocator.Current.GetInstance<EPiServer.Web.Routing.PageRouteHelper>();
                    currentPage = pageRouteHelper.Page;
                }

                return (T)currentPage;
            }
        }

        /// <summary>
        /// Transforms the LinkItemCollection object of children into a System.Generic.Collections.List object
        /// that contains PageData objects for each of the children that were in the childCollection parameter.
        /// </summary>
        /// <param name="childCollection">The LinkItemCollection object that contains all of the pagedata objects
        /// that are associated with this page.</param>
        /// <param name="publishedPagesOnly">Boolean which determines if only published pages are returned.</param>
        /// <returns>A System.Generic.Collections.List of PageData object that contains PageData objects for each of
        /// the children that were in the childCollection parameter.</returns>
        protected List<PageData> GetModuleList(LinkItemCollection childCollection, bool publishedPagesOnly)
        {
            return childCollection.ConvertToPageDataList(publishedPagesOnly);
        }

        /// <summary>
        /// Fetch a generic list that contains all child pages (of the referenced page) that are of a particular 
        /// page type.
        /// </summary>
        /// <typeparam name="U">The type of child pages to retrieve.</typeparam>
        /// <param name="pageLink"></param>
        /// <returns>A generic list of child pages.</returns>
        protected List<U> FetchAllChildren<U>(PageReference pageLink) where U : TypedPageData
        {
            if (pageLink != null)  // Check for null.
            {
                return DataFactory.Instance.GetChildrenOfType<U>(pageLink).ToList();
            }

            return new List<U>();
        }

        protected string GetProxyBaseClassName(ContentData data)
        {
            var className = string.Empty;

            var proxy = data as IProxyTargetAccessor;
            if (proxy != null)
            {
                var target = proxy.DynProxyGetTarget();
                if (target != null)
                {
                    var baseType = target.GetType().BaseType;
                    if (baseType != null)
                    {
                        className = baseType.Name;
                    }
                }
            }

            return className;
        }

        public BlockData GetContentReferenceContent(ContentReference contentReferenceItem)
        {
            return ContentRepository.Get<EPiServer.Core.BlockData>(contentReferenceItem);
        }
    }
}
