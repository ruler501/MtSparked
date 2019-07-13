using MtSparked.Models;
using MtSparked.Services;
using MtSparked.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MtSparked.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DomainCriteriaCell : ContentView
    {
        private DomainCriteria DomainCriteria { get; set; }

        public DomainCriteriaCell(DomainCriteria criteria)
        {
            InitializeComponent();

            this.BindingContext = DomainCriteria = criteria;
        }
    }
}