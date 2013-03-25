/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Axiom.MathLib;
using Axiom.Animating;
using Axiom.Collections;

namespace Multiverse.Tools.ModelViewer {
    public partial class BoneDisplay : Form {
        SkeletonInstance skeleton;
        Skeleton helperSkeleton;
        string selectedBone = null;
        public ModelViewer modelViewer;

        public BoneDisplay() {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(this.BoneDisplay_Close);
        }

        public BoneDisplay(ModelViewer parent) : this() {
            modelViewer = parent;
        }
         
        private void BoneDisplay_Load(object sender, EventArgs e) {

        }

        private void BoneDisplay_Close(object sender, FormClosedEventArgs e) {
            if (modelViewer != null)
                modelViewer.boneDisplay = null;
        }

        public void SetSkeleton(SkeletonInstance skel) {
            Bone bone = skel.RootBone;
            TreeNode node = new TreeNode();
            node.Name = bone.Name;
            node.Text = bone.Name;
            boneTreeView.Nodes.Add(node);
            AddChildBones(node, bone);

            skeleton = skel;
            string skeletonName = skel.MasterSkeleton.Name;
            helperSkeleton = SkeletonManager.Instance.Load(skeletonName);
        }

        private void AddChildBones(TreeNode node, Bone bone) {
            foreach (Bone childBone in bone.Children) {
                TreeNode childNode = new TreeNode();
                childNode.Name = childBone.Name;
                childNode.Text = childBone.Name;
                node.Nodes.Add(childNode);
                AddChildBones(childNode, childBone);
            }
        }

        public void UpdateFields() {
            boneNameLabel.Text = selectedBone;
            if (skeleton.ContainsBone(boneNameLabel.Text)) {
                Bone bone = skeleton.GetBone(boneNameLabel.Text);
                Quaternion q = ModelViewer.GetRotation(bone.FullTransform);
                boneNameLabel.Text = bone.Name;
                bonePosition.Text = bone.FullTransform.Translation.ToString();
                boneRotation.Text = q.ToString();
                boneRotation2.Text = string.Format("X:{0} Y:{1} Z:{2}", q.PitchInDegrees, q.YawInDegrees, q.RollInDegrees);
                q = bone.Orientation;
                relPosition.Text = bone.Position.ToString();
                relRotation.Text = q.ToString();
                relRotation2.Text = string.Format("X:{0} Y:{1} Z:{2}", q.PitchInDegrees, q.YawInDegrees, q.RollInDegrees);
                Bone bindBone = skeleton.GetBone(boneNameLabel.Text);
                Matrix4 bindBoneFullTransform = bindBone.BindDerivedInverseTransform.Inverse();
                q = ModelViewer.GetRotation(bindBoneFullTransform);
                bindPosition.Text = bindBoneFullTransform.Translation.ToString();
                bindRotation.Text = q.ToString();
                bindRotation2.Text = string.Format("X:{0} Y:{1} Z:{2}", q.PitchInDegrees, q.YawInDegrees, q.RollInDegrees);
                AnimationState currentAnimation = modelViewer.CurrentAnimation;
                if (currentAnimation != null) {
                    animationName.Text = "Animation: " + currentAnimation.Name;
                    float currentTime = currentAnimation.Time;
                    animationTime.Text = "Time: " + currentTime.ToString();
                    Animation anim = skeleton.GetAnimation(currentAnimation.Name);
                    NodeAnimationTrack animTrack = anim.NodeTracks[bone.Handle];
                    KeyFrame keyFrame1, keyFrame2;
                    ushort dummy;
                    animTrack.GetKeyFramesAtTime(currentTime, out keyFrame1, out keyFrame2, out dummy);
                    AnimationStateSet helperAnimSet = new AnimationStateSet();
                    helperSkeleton.InitAnimationState(helperAnimSet);
                    AnimationState helperAnimation = helperAnimSet.GetAnimationState(currentAnimation.Name);
                    helperAnimation.IsEnabled = true;
                    helperAnimation.Time = keyFrame1.Time;
                    helperSkeleton.SetAnimationState(helperAnimSet);
                    Bone helperBone = helperSkeleton.GetBone(bone.Handle);
                    // currentAnimation.Time;
                    keyFrame1Time.Text = helperAnimation.Time.ToString();
                    q = ModelViewer.GetRotation(helperBone.FullTransform);
                    keyFrame1Position.Text = helperBone.FullTransform.Translation.ToString();
                    keyFrame1Rotation.Text = q.ToString();
                    keyFrame1Rotation2.Text = string.Format("X:{0} Y:{1} Z:{2}", q.PitchInDegrees, q.YawInDegrees, q.RollInDegrees);
                    helperAnimation.Time = keyFrame2.Time;
                    helperSkeleton.SetAnimationState(helperAnimSet);
                    keyFrame2Time.Text = helperAnimation.Time.ToString();
                    q = ModelViewer.GetRotation(helperBone.FullTransform);
                    keyFrame2Position.Text = helperBone.FullTransform.Translation.ToString();
                    keyFrame2Rotation.Text = q.ToString();
                    keyFrame2Rotation2.Text = string.Format("X:{0} Y:{1} Z:{2}", q.PitchInDegrees, q.YawInDegrees, q.RollInDegrees);
#if NOT
                    keyFrame1Time.Text = keyFrame1.Time.ToString();
                    q = keyFrame1.Rotation;
                    keyFrame1Position.Text = helperBone.Translate.ToString();
                    keyFrame1Rotation.Text = q.ToString();
                    keyFrame1Rotation2.Text = string.Format("X:{0} Y:{1} Z:{2}", q.PitchInDegrees, q.YawInDegrees, q.RollInDegrees);
                    keyFrame2Time.Text = keyFrame2.Time.ToString();
                    q = keyFrame2.Rotation;
                    keyFrame2Position.Text = keyFrame2.Translate.ToString();
                    keyFrame2Rotation.Text = q.ToString();
                    keyFrame2Rotation2.Text = string.Format("X:{0} Y:{1} Z:{2}", q.PitchInDegrees, q.YawInDegrees, q.RollInDegrees);
#endif
                } else {
                    animationName.Text = "No animation selected";
                }
                bonePosition.Show();
                boneRotation.Show();
                boneRotation2.Show();
                relPosition.Show();
                relRotation.Show();
                relRotation2.Show();
                bindPosition.Show();
                bindRotation.Show();
                bindRotation2.Show();
                if (currentAnimation != null) {
                    prevKeyframeLabel.Show();
                    keyFrame1TimeLabel.Show();
                    keyFrame1PositionLabel.Show();
                    keyFrame1RotationLabel.Show();
                    keyFrame1Time.Show();
                    keyFrame1Position.Show();
                    keyFrame1Rotation.Show();
                    keyFrame1Rotation2.Show();
                    nextKeyframeLabel.Show();
                    keyFrame2TimeLabel.Show();
                    keyFrame2PositionLabel.Show();
                    keyFrame2RotationLabel.Show();
                    keyFrame2Time.Show();
                    keyFrame2Position.Show();
                    keyFrame2Rotation.Show();
                    keyFrame2Rotation2.Show();
                } else {
                    prevKeyframeLabel.Hide();
                    keyFrame1TimeLabel.Hide();
                    keyFrame1PositionLabel.Hide();
                    keyFrame1RotationLabel.Hide();
                    keyFrame1Time.Hide();
                    keyFrame1Position.Hide();
                    keyFrame1Rotation.Hide();
                    keyFrame1Rotation2.Hide();
                    nextKeyframeLabel.Hide();
                    keyFrame2TimeLabel.Hide();
                    keyFrame2PositionLabel.Hide();
                    keyFrame2RotationLabel.Hide();
                    keyFrame2Time.Hide();
                    keyFrame2Position.Hide();
                    keyFrame2Rotation.Hide();
                    keyFrame2Rotation2.Hide();
                }
            } else {
                boneNameLabel.Text = "Invalid Bone Selected";
                bonePosition.Hide();
                boneRotation.Hide();
                boneRotation2.Hide();
                relPosition.Hide();
                relRotation.Hide();
                relRotation2.Hide();
                bindPosition.Hide();
                bindRotation.Hide();
                bindRotation2.Hide();
                prevKeyframeLabel.Hide();
                keyFrame1TimeLabel.Hide();
                keyFrame1PositionLabel.Hide();
                keyFrame1RotationLabel.Hide();
                keyFrame1Time.Hide();
                keyFrame1Position.Hide();
                keyFrame1Rotation.Hide();
                keyFrame1Rotation2.Hide();
                nextKeyframeLabel.Hide();
                keyFrame2TimeLabel.Hide();
                keyFrame2PositionLabel.Hide();
                keyFrame2RotationLabel.Hide();
                keyFrame2Time.Hide();
                keyFrame2Position.Hide();
                keyFrame2Rotation.Hide();
                keyFrame2Rotation2.Hide();
            }
        }

        private void boneTreeView_AfterSelect(object sender, TreeViewEventArgs e) {
            selectedBone = e.Node.Text;
            UpdateFields();
        }
    }
}
