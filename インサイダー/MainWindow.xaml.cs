using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace インサイダー {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow:Window {
		public MainWindow() {
			InitializeComponent();
			this.answersTime.ValueChanged+=new RoutedEventHandler(this.answersTime_ValueChanged);
			this.AnswersTimer.Tick+=new EventHandler(this.AnswersTimerUpdate);
			this.DiscussionTimer.Tick+=new EventHandler(this.DiscussionTimerUpdate);
		}

		//private Random dice1 = new Random();
		//private Random dice2 = new Random();
		private RNGCryptoServiceProvider dice1 = new RNGCryptoServiceProvider();
		private RNGCryptoServiceProvider dice2 = new RNGCryptoServiceProvider();

		private eisiWare.NumericUpDown player = new eisiWare.NumericUpDown() {
			MinValue=4,
			MaxValue=20,
			HorizontalAlignment=HorizontalAlignment.Right,
			Value=4,
			FontSize=18,
			Margin=new Thickness(0,0,2,0),
			FontFamily=new FontFamily("Meiryo UI")
		};

		private void Window_Loaded(object sender,RoutedEventArgs e) {
			this.playersGrid.Children.Add(this.player);
			this.AnswersTimeGrid.Children.Add(this.answersTime);
		}

		private int insideNumber;
		private void insiderButton_Click(object sender,RoutedEventArgs e) {
			byte[] val = new byte[1];
			dice1.GetNonZeroBytes(val);
			this.insideNumber=(int)Math.Ceiling(val[0]*this.player.Value/byte.MaxValue);
			//this.insideNumber=(int)(Math.Ceiling(this.dice1.NextDouble()*(this.player.Value)));
			this.insiderPlayerNumberLabel.Content=this.insideNumber.ToString(new CultureInfo("ja-JP"))+"番";
			this.masterReset();
			this.masterButton.IsEnabled=true;
		}
		/*private void insiderReset() {
			this.masterReset();
		}*/

		private int masterNumber;
		private void masterButton_Click(object sender,RoutedEventArgs e) {
			byte[] val = new byte[1];
			dice2.GetNonZeroBytes(val);
			this.masterNumber=(int)Math.Ceiling(val[0]*(this.player.Value-1)/byte.MaxValue);
			//this.masterNumber=(int)(Math.Ceiling(this.dice2.NextDouble()*(this.player.Value-1)));
			if(this.masterNumber>=this.insideNumber) {
				this.masterNumber++;
			}
			this.masterPlayerNumberLabel.Content=this.masterNumber.ToString(new CultureInfo("ja-JP"))+"番";
			this.AnswersTimerReset();
			this.AnswersTimerStartButton.IsEnabled=true;
		}

		private void masterReset() {
			this.masterPlayerNumberLabel.Content="未抽選";
			this.AnswersTimerReset();
		}

		private eisiWare.NumericUpDown answersTime = new eisiWare.NumericUpDown() {
			MinValue=1,
			MaxValue=60,
			HorizontalAlignment=HorizontalAlignment.Right,
			Value=5,
			FontSize=18,
			Margin=new Thickness(0,0,2,0),
			FontFamily=new FontFamily("Meiryo UI")
		};
		private void answersTime_ValueChanged(object sender,RoutedEventArgs e) {
			var ate = this.AnswersTimerStartButton.IsEnabled||this.AnswersTimerResetButton.IsEnabled;
			this.AnswersTimerReset();
			this.AnswersTimerStartButton.IsEnabled=ate;
		}

		private DateTime AnswersStartTime;
		private DateTime AnswersEndTime;
		private DispatcherTimer AnswersTimer=new DispatcherTimer();
		private bool AnswersTimerStarted = false;
		private void AnswersTimerStartButton_Click(object sender,RoutedEventArgs e) {
			this.AnswersTimerResetButton.IsEnabled=true;
			if(this.AnswersTimerStarted) {
				this.AnswersTimer.IsEnabled=false;
				this.AnswersEndTime=DateTime.Now;
				this.AnswersTimerStartButton.IsEnabled=false;
				this.DiscussionTimerLabel.Content=this.AnswersEndTime.Subtract(this.AnswersStartTime).ToString();
				this.DiscussionTimerStartButton.IsEnabled=true;
			} else {
				this.AnswersStartTime=DateTime.Now;
				this.AnswersEndTime=this.AnswersStartTime.AddMinutes(this.answersTime.Value);
				this.AnswersTimerStartButton.Content="回答終了";
				this.AnswersTimerStarted=true;
				this.AnswersTimer.IsEnabled=true;
			}
		}

		private void AnswersTimerResetButton_Click(object sender,RoutedEventArgs e) {
			this.AnswersTimerReset();
			this.AnswersTimerStartButton.IsEnabled=true;
		}
		private void AnswersTimerReset() {
			this.AnswersTimer.IsEnabled=false;
			this.AnswersTimerStartButton.Content="回答開始";
			this.AnswersTimerStartButton.IsEnabled=false;
			this.AnswersTimerResetButton.IsEnabled=false;
			this.AnswersTimerStarted=false;
			this.AnswersTimerLabel.Content=new TimeSpan(0,(int)this.answersTime.Value,0).ToString()+".0000";
			this.DiscussionTimerReset();
		}
		private void AnswersTimerUpdate(object sender,EventArgs e) {
			if(this.AnswersEndTime<=DateTime.Now) {
				this.AnswersTimer.IsEnabled=false;
				this.AnswersTimerStartButton.IsEnabled=false;
				this.DiscussionTimerStartButton.IsEnabled=true;
				this.AnswersTimerLabel.Content=new TimeSpan(0,0,0).ToString()+".0000";
			} else {
				this.AnswersTimerLabel.Content=this.AnswersEndTime.Subtract(DateTime.Now).ToString();
			}
		}
		private DateTime DiscussionStartTime;
		private DateTime DiscussionEndTime;
		private DispatcherTimer DiscussionTimer = new DispatcherTimer();
		private bool DiscussionTimerStarted = false;
		private void DiscussionTimerStartButton_Click(object sender,RoutedEventArgs e) {
			this.DiscussionTimerResetButton.IsEnabled=true;
			if(this.DiscussionTimerStarted) {
				this.DiscussionTimer.IsEnabled=false;
				this.DiscussionTimerStartButton.IsEnabled=false;
			} else {
				this.DiscussionStartTime=DateTime.Now;
				this.DiscussionEndTime=this.DiscussionStartTime.Add(this.AnswersEndTime.Subtract(this.AnswersStartTime));
				this.DiscussionTimerStartButton.Content="議論終了";
				this.DiscussionTimerStarted=true;
				this.DiscussionTimer.IsEnabled=true;
			}
		}
		private void DiscussionTimerResetButton_Click(object sender,RoutedEventArgs e) {
			this.DiscussionTimerReset();
			this.DiscussionTimerStartButton.IsEnabled=true;
		}
		private void DiscussionTimerReset() {
			this.DiscussionTimer.IsEnabled=false;
			this.DiscussionTimerStartButton.Content="議論開始";
			this.DiscussionTimerResetButton.IsEnabled=false;
			this.DiscussionTimerStartButton.IsEnabled=false;
			this.DiscussionTimerStarted=false;
			if(this.AnswersTimerStarted) {
				this.DiscussionTimerLabel.Content=this.AnswersEndTime.Subtract(this.AnswersStartTime).ToString();
			} else{
				this.DiscussionTimerLabel.Content=new TimeSpan(0,(int)this.answersTime.Value,0).ToString()+".0000";
			}
				
		}
		private void DiscussionTimerUpdate(object sender,EventArgs e) {
			if(this.DiscussionEndTime<=DateTime.Now) {
				this.DiscussionTimer.IsEnabled=false;
				this.DiscussionTimerStartButton.IsEnabled=false;
				this.DiscussionTimerLabel.Content=new TimeSpan(0,0,0).ToString()+".0000";
			} else {
				this.DiscussionTimerLabel.Content=this.DiscussionEndTime.Subtract(DateTime.Now).ToString();
			}
		}
	}
}
